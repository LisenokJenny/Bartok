using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour {
	[Header("Set Dynamically")]
	public string suit; // масть карты (C, D, H или S)
	public int rank; // достоинство карты (1-14)
	public Color color = Color.black; // цвет значков
	public string colS = "Black"; // или "Red". Имя цвета
	// этот список хранит все игровые объекты Decorator
	public List<GameObject> decoGOs = new List<GameObject>();
	// этот список хранит все игровые объекты Pip
	public List<GameObject> pipGOs = new List<GameObject>();

	public GameObject back; // игровой объект рубашки карты

	public CardDefinition def; // извлекается из DeckXML.xml

	// список компонентов SpriteRenderer этого и вложенных в него игровых объектов
	public SpriteRenderer[] spriteRenderers;

	void Start() {
		SetSortOrder(0); // обеспечит правильную сортировку карт
	}

	// если spriteRenderers не определён, эта функция определит его
	public void PopulateSpriteRenderers() {
		// если spriteRenderers содержит null или пустой список
		if (spriteRenderers == null || spriteRenderers.Length == 0) {
			// получить компоненты SpriteRenderer этого игрового объекта и вложенных в него игровых объектов
			spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		}
	}

	// инициализирует поле sortingLayerName во всех компонентах SpriteRenderer
	public void SetSortingLayerName(string tSLN) {
		PopulateSpriteRenderers();

		foreach (SpriteRenderer tSR in spriteRenderers) {
			tSR.sortingLayerName = tSLN;
		}
	}

	// инициализирует поле sortingOrder всех компонентов SpriteRenderer
	public void SetSortOrder(int sOrd) {
		PopulateSpriteRenderers ();

		// выполнить обход всех элементов в списке spriteRenderers
		foreach (SpriteRenderer tSR in spriteRenderers) {
			if (tSR.gameObject == this.gameObject) {
				// если компонент принадлежит текущему игровому объекту, это фон
				tSR.sortingOrder = sOrd; // установить порядковый номер для сортировки в sOrd
				continue; // и перейти к следующей итерации цикла
			}

			// каждый дочерний игровой объект имеет имя
			// установить порядковый номер для сортировки, в зависимости от имени
			switch (tSR.gameObject.name) {
			case "back": // если имяя "back", установить наибольший порядковый номер для отображения поверх других спрайтов
				tSR.sortingOrder = sOrd + 2;
				break;

			case "face": // если имя "face"
			default: // или же другое
				// установить промежуточный порядковый номер для отображения поверх фона
				tSR.sortingOrder = sOrd + 1;
				break;
			}
		}
	}

	public bool faceUp {
		get {
			return(!back.activeSelf);
		}
		set {
			back.SetActive (!value);
		}
	}

	// виртуальные методы могут переопределяться в подклассах определением методов с теми же именами
	virtual public void OnMouseUpAsButton() {
		print (name); // по щелчку эта строка выведет имя карты
	}
}

[System.Serializable] // сериализуемый класс доступен для правки в инспекторе
public class Decorator {
	// этот класс хранит информацию из DeckXML о каждом значке на карте
	public string type; // значок, определяющий достоинство карты
						// имеет typr = "pip"
	public Vector3 loc; // местоположение спрайта на карте
	public bool flip = false; // признак переворота спрайта по вертикали
	public float scale = 1f; // масштаб спрайта
}

[System.Serializable]
public class CardDefinition {
	// этот класс хранит информацию о достоинстве карты
	public string face; // спрайт, изображающий лицевую сторону карты
	public int rank; // достоинство карты (1-13)
	public List<Decorator> pips = new List<Decorator>(); // значки
}
