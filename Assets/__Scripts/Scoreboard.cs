using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// класс Scoreboard управляет отображением очков игрока

public class Scoreboard : MonoBehaviour {

	public static Scoreboard S; // это объект-одиночка Scoreboard

	[Header("Set in Inspector")]
	public GameObject prefabFloatingScore;

	[Header("Set Dynamically")]
	[SerializeField] private int _score = 0;
	[SerializeField] private string _scoreString;

	private Transform canvasTrans;

	// свойство score также устанавливает scoreString
	public int score {
		get {
			return(_score);
		}
		set {
			_score = value;
			scoreString = _score.ToString ("N0");
		}
	}

	// свойство scoreString также устанавливает Text.text
	public string scoreString {
		get {
			return(_scoreString);
		}
		set {
			_scoreString = value;
			GetComponent<Text>().text = _scoreString;
		}
	}

	void Awake() {
		if (S == null) {
			S = this; // подготовка скрытого объекта-одиночки
		} else {
			Debug.LogError ("ERROR: Scoreboard.Awake(): S is already set!");
		}
		canvasTrans = transform.parent;
	}

	// когда вызывается методом SendMessage, прибавляет fs.score к this.score
	public void FSCallback(FloatingScore fs) {
		score += fs.score;
	}

	// создаёт и инициализирует новый игровой объект FloatingScore
	// возвращает указатель на созданный экземпляр FloatingScore, чтобы вызывающая функция могла выполнить с ним дополнительные операции
	// (например, определить список fontSizes и т.д.)
	public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts) {
		GameObject go = Instantiate <GameObject> (prefabFloatingScore);
		go.transform.SetParent( canvasTrans );
		FloatingScore fs = go.GetComponent<FloatingScore>();
		fs.score = amt;
		fs.reportFinishTo = this.gameObject; // настроить обратный вызов
		fs.Init(pts);
		return(fs);
	}
}
