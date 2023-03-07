using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//перечисление со всеми возможными состояниями FloatingScore
public enum eFSState {
	idle,
	pre,
	active,
	post
}

// FloatingScore может перемещаться на экране по траектории, которая определяется кривой Безье

public class FloatingScore : MonoBehaviour {

	[Header("Set Dynamically")]
	public eFSState state = eFSState.idle;

	[SerializeField]
	protected int _score = 0;
	public string scoreString;

	// свойство score устанавливает два поля, _score и scoreString
	public int score {
		get {
			return(_score);
		}
		set {
			_score = value;
			scoreString = _score.ToString("N0"); // аргумент "N0" требует добавить точки в число
												  // !!! поищите в интернете по фразе "C# Строки стандартных цифровых форматов", чтобы нацти описание форматов, поддерживаемых методом ToString
			GetComponent<Text>().text = scoreString;
		}
	}

	public List<Vector2> bezierPts; // точки, определяющие кривую Безье
	public List<float> fontSizes; // точки кривой Безье для масштабирования шрифта
	public float timeStart = -1f;
	public float timeDuration = 1f;
	public string easingCurve = Easing.InOut; // функция сглаживания из Utils.cs

	// игровой объект, для которого будет вызван метод SendMassage, когда этот экземпляр FloatingScore закончит движение
	public GameObject reportFinishTo = null;

	private RectTransform rectTrans;
	private Text txt;

	// настроить FloatindScore и параметры движения
	// обратите внимание, что для параметров eTimeS и eTimeD определены значения по умолчанию

	public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1) {
		rectTrans = GetComponent<RectTransform> ();
		rectTrans.anchoredPosition = Vector2.zero;

		txt = GetComponent<Text>();

		bezierPts = new List<Vector2> (ePts);

		if (ePts.Count == 1) { // если задана только одна точка, просто переместиться в неё
			transform.position = ePts[0];
			return;
		}

		// если eTimeS имеет значение по умолчанию, запустить отсчёт от текущего времени
		if (eTimeS == 0) eTimeS = Time.time;
		timeStart = eTimeS;
		timeDuration = eTimeD;

		state = eFSState.pre; // установить состояние pre - готовность начать движение
	}

	public void FSCallback(FloatingScore fs) {
		// когда SendMessage вызовет эту функцию, она должна добавить очки из вызвавшего экземпляра FloatingScore
		score += fs.score;
	}
	
	// Update вызывается в каждом кадре
	void Update () {
		// если этот объект никуда не перемещается, просто выйти
		if (state == eFSState.idle) return;

		// вычислить u на основе текущего времени и продолжительности движения
		// u изменяется от 0 до 1 (обычно
		float u = (Time.time - timeStart)/timeDuration;
		// использовать класс Easing из Utils для корректировки значения u
		float uC = Easing.Ease (u, easingCurve);
		if (u<0) { // если u<0, объект не должен двигаться
			state = eFSState.pre;
			txt.enabled = false; // изначально скрыть число
		} else {
			if (u>=1) {// если u>=1, выполняется движение
				uC = 1; // установить uC=1, чтобы не выйти за крайнюю точку
				state = eFSState.post;
				if (reportFinishTo != null) { // если игровой объект указан, использовать SandMessage для вызова метода FSCallback и передачи ему текущего экземпляра в параметре
					reportFinishTo.SendMessage ("FSCallback", this);
					// после отправки сообщения уничтожить gameObject
					Destroy (gameObject);
				} else { // если не указан, не уничтожать текущий экземпляр. Просто оставить его в покое
					state = eFSState.idle;
				}
			} else {
			// если 0<=u<1, значит, текущий экземпляр активен и движется
				state = eFSState.active;
				txt.enabled = true; // показать число очков
			}

			// использовать кривую Безье для перемещения к заданной точке
			Vector2 pos = Utils.Bezier(uC, bezierPts);
			// опорные точки RectTransform можно использовать для позиционирования объектов пользовательского интерфейса относительно общего размера экрана
			rectTrans.anchorMin = rectTrans.anchorMax = pos;
			if (fontSizes != null && fontSizes.Count>0) {
				// если список fontSizes содержит значения, скорректировать fontSize этого объекта GUIText
				int size = Mathf.RoundToInt( Utils.Bezier(uC, fontSizes) );
				GetComponent<Text>().fontSize = size;
			}
		}
	}
}
