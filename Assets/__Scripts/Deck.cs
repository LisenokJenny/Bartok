using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.Xml;
using System.IO;

public class Deck : MonoBehaviour {

	[Header("Set in Inspector")]

	public bool startFaceUp = false;

	// масти
	public Sprite suitClub;
	public Sprite suitDiamond;
	public Sprite suitHeart;
	public Sprite suitSpade;

	public Sprite[] faceSprites;
	public Sprite[] rankSprites;

	public Sprite cardBack;
	public Sprite cardBackGold;
	public Sprite cardFront;
	public Sprite cardFrontGold;

	// шаблоны
	public GameObject prefabCard;
	public GameObject prefabSprite;

	[Header("Set Dynamically")]
	public PT_XMLReader xmlr;
	public List<string> cardNames;
	public List<Card> cards;
	public List<Decorator> decorators;
	public List<CardDefinition> cardDefs;
	public Transform deckAnchor;
	public Dictionary<string,Sprite> dictSuits;

	// InitDeck вызывается экземпляром Prospector, когда будет готов
	public void InitDeck(string deckXMLText) {
		// создать точку привязки для всех игровых объектов Card в иерархии
		if (GameObject.Find("_Deck") == null) {
			GameObject anchorGO = new GameObject ("_Deck");
			deckAnchor = anchorGO.transform;
		}

		// инициализировать словарь со спрайтами значков мастей
		dictSuits = new Dictionary<string, Sprite> () {
			{ "C", suitClub },
			{ "D", suitDiamond },
			{ "H", suitHeart },
			{ "S", suitSpade }
		};
		ReadDeck(deckXMLText);
		MakeCards();
	}

	// ReadDeck читает указанный XML-файл и создаёт массив экземпляров CardDefinition
	public void ReadDeck(string deckXMLText) { 
		xmlr = new PT_XMLReader(); // создать новый экземпляр PT_XMLReader
		xmlr.Parse(deckXMLText); // использовать его для чтения DeckXML

		// вывод проверочной строки, чтобы показать, как использовать xmlr
		string s = "xml[0] decorator[0] ";
		s += "type=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("type");
		s += " x=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("x");
		s += " y=" + xmlr.xml["xml"] [0] ["decorator"] [0].att ("y");
		s += " scale=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("scale");
		// print (s); // закомментируйте эту строку, так как мы уже закончили тестирование

		// прочитать элементы <decorator> для всех карт
		decorators = new List<Decorator>(); // инициализировать список экземпляров Decorator
		// извлечь список PT_XMLHashList всех элементов <decorator> из XML-файла
		PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
		Decorator deco;
		for (int i = 0; i < xDecos.Count; i++) {
			// для каждого элемента <decorator> в XML
			deco = new Decorator(); // создать экземпляр Decorator;
			// скопировать атрибуты из <decorator> в Decorator
			deco.type = xDecos[i].att("type");
			// deco.fli получает значение true, если атрибут flip содержит текст "1"
			deco.flip = ( xDecos[i].att ("flip") == "1" );
			// получить значения float из строковых атрибутов
			deco.scale = float.Parse( xDecos[i].att ("scale") );
			// Vector3 loc инициализируется как [0,0,0], поэтому нам остаётся только изменить его
			deco.loc.x = float.Parse( xDecos[i].att ("x") );
			deco.loc.y = float.Parse( xDecos[i].att ("y") );
			deco.loc.z = float.Parse( xDecos[i].att ("z") );
			// добавить deco в список decorators
			decorators.Add (deco);
		}

		// прочитать координаты для значков, определяющих достоинство карты
		cardDefs = new List<CardDefinition>(); // инициализировать список карты
		// извлечь список PT_XMLHashList всех элементов <card> из XML-файла
		PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
		for (int i = 0; i < xCardDefs.Count; i++) {
			// для каждого элемента <card> создать экземпляр CardDefinition
			CardDefinition cDef = new CardDefinition();
			// получить значения атрибута и добавить их в cDef
			cDef.rank = int.Parse( xCardDefs[i].att ("rank") );
			// извлечь список PT_XMLHashList всех элементов <pip> внутри этого элемента <card>
			PT_XMLHashList xPips = xCardDefs[i]["pip"];
			if (xPips != null) {
				for (int j=0; j<xPips.Count; j++) {
					// обойти все элементы <pip>
					deco = new Decorator();
					// элементы <pip> в <card> обрабатываются классом Decorator
					deco.type = "pip";
					deco.flip = ( xPips[j].att ("flip") == "1" );
					deco.loc.x = float.Parse( xPips[j].att ("x") );
					deco.loc.y = float.Parse( xPips[j].att ("y") );
					deco.loc.z = float.Parse( xPips[j].att ("z") );
					if ( xPips[j].HasAtt("scale") ) {
						deco.scale = float.Parse( xPips[j].att ("scale") );
					}
					cDef.pips.Add(deco);
				}
			}
			// карты с картинками (Валет, Дама и Король) имеют атрибут face
			if (xCardDefs[i].HasAtt("face")) {
				cDef.face = xCardDefs[i].att ("face");
			}
			cardDefs.Add(cDef);
		}
	}

	// получает CardDefinition на основе значения достоинства (от 1 до 14 - от туза до короля)
	public CardDefinition GetCardDefinitionByRank(int rnk) {
		// поиск во всех определениях CardDefinition
		foreach (CardDefinition cd in cardDefs) {
			// если достоинство совпадает, вернуть это определение
			if (cd.rank == rnk) {
				return(cd);
			}
		}
		return(null);
	}

	// создаёт игровые объекты карт
	public void MakeCards() {
		// cardNames будет создавать имена сконструированных карт
		// каждая масть имеет 14 значений достоинства (например, для труф (Clubs): от C1 до C14)
		cardNames = new List<string>();
		string[] letters = new string[] { "C", "D", "H", "S" };
		foreach (string s in letters) {
			for (int i = 0; i < 13; i++) {
				cardNames.Add (s + (i + 1));
			}
		}

		// создать список со всеми картами
		cards = new List<Card>();

		// обойти все только что созданные имена карт
		for (int i=0; i<cardNames.Count; i++) {
			// создать карту и добавить её в колоду
			cards.Add ( MakeCard(i) );
		}
	}

	private Card MakeCard(int cNum) {
		// создать новый игровой объект с картой
		GameObject cgo = Instantiate(prefabCard) as GameObject;
		// настроить transform.parent новой карты в соответствии с точкой привязки
		cgo.transform.parent = deckAnchor;
		Card card = cgo.GetComponent<Card>(); // получить компонент Card

		// эта строка выкладывает карты в аккуратный ряд
		cgo.transform.localPosition = new Vector3( (cNum%13)*3, cNum/13*4, 0 );

		// настроить основные параметры карты
		card.name = cardNames[cNum];
		card.suit = card.name[0].ToString();
		card.rank = int.Parse(card.name.Substring (1));
		if (card.suit == "D" || card.suit == "H") {
			card.colS = "Red";
			card.color = Color.red;
		}
		//  получить CardDefinition для этой карты
		card.def = GetCardDefinitionByRank(card.rank);

		AddDecorators(card);
		AddPips(card);
		AddFace(card);
		AddBack(card);

		return card;
	}

	// следующие скрытые переменные используются вспомогательными методами
	private Sprite _tSp = null;
	private GameObject _tGO = null;
	private SpriteRenderer _tSR = null;

	private void AddDecorators(Card card) {
		// добавить оформление
		foreach( Decorator deco in decorators ) {
			if (deco.type == "suit") {
				// создать экземпляр игрового объекта спрайта
				_tGO = Instantiate (prefabSprite) as GameObject;
				// получить ссылку на компонент SpriteRenderer
				_tSR = _tGO.GetComponent<SpriteRenderer> ();
				// установить спрайт масти
				_tSR.sprite = dictSuits [card.suit];
			} else {
				_tGO = Instantiate (prefabSprite) as GameObject;
				_tSR = _tGO.GetComponent<SpriteRenderer> ();
				// получить спрайт для отображения достоинства
				_tSp = rankSprites[ card.rank ];
				// установить спрайт достоинства в SpriteRenderer
				_tSR.sprite = _tSp;
				// установить цвет соответствующий масти
				_tSR.color = card.color;
			}
			// поместить спрайты над картой
			_tSR.sortingOrder = 1;
			// сделать спрайт дочерним по отношению к карте
			_tGO.transform.SetParent( card.transform );
			// установить localPosition, как определено в DeckXML
			_tGO.transform.localPosition = deco.loc;
			// перевернуть значок, если необходимо
			if (deco.flip) {
				// Эйлеров поворот на 180 градусов относительно оси Z-axis
				_tGO.transform.rotation = Quaternion.Euler(0,0,180);
			}
			// установить масштаб, чтобы уменьшить размер спрайта
			if (deco.scale != 1) {
				_tGO.transform.localScale = Vector3.one * deco.scale;
			}
			// дать имя этому игровому объекту для наглядности
			_tGO.name = deco.type;
			// добавить этот игровой объект с оформлением в список card.decoGOs
			card.decoGOs.Add(_tGO);
		}
	}

	private void AddPips(Card card) {
		// для каждого значка в определении...
		foreach( Decorator pip in card.def.pips ) {
			// создать игровой объект спрайта
			_tGO = Instantiate( prefabSprite ) as GameObject;
			// назначить родителем игровой объект карты
			_tGO.transform.SetParent( card.transform );
			// установить localPosition, как определено в XML-файле
			_tGO.transform.localPosition = pip.loc;
			// перевернуть, если необходимо
			if (pip.flip) {
				_tGO.transform.rotation = Quaternion.Euler (0, 0, 180);
			}
			// масштабировать, если необходимо (только для туза)
			if (pip.scale != 1) {
				_tGO.transform.localScale = Vector3.one * pip.scale;
			}
			// дать имя игровому объекту
			_tGO.name = "pip";
			// получить ссылку на компонент SpriteRenderer
			_tSR = _tGO.GetComponent<SpriteRenderer>();
			// установить спрайт масти
			_tSR.sprite = dictSuits[card.suit];
			// установить sortingOrder, чтобы значок отображался над Card_Front
			_tSR.sortingOrder = 1;
			// добавить этот игровой объект в список значков
			card.pipGOs.Add(_tGO);
		}
	}

	private void AddFace(Card card) {
		if (card.def.face == "") {
			return; // выйти, если это не карта с картинкой
		}

		_tGO = Instantiate (prefabSprite) as GameObject;
		_tSR = _tGO.GetComponent<SpriteRenderer> ();
		// сгенерировать имя и передать его в GetFace()
		_tSp = GetFace( card.def.face+card.suit );
		_tSR.sprite = _tSp; // установить этот спрайт в _tSR
		_tSR.sortingOrder = 1; // установить sortingOrder
		_tGO.transform.SetParent( card.transform );
		_tGO.transform.localPosition = Vector3.zero;
		_tGO.name = "face";
	}
		
	// находит спрайт с картинкой для карты
	private Sprite GetFace(string faceS) {
		foreach (Sprite _tSP in faceSprites) {
			// если найден спрайт с требуемым именем...
			if(_tSP.name == faceS) {
				// ... вернуть его
				return( _tSP );
			}
		}
		// если ничего не найдено, вернуть null
		return( null );
	}

	private void AddBack(Card card) {
		// добавить рубашку
		// Card_Back будет покрывать всё остальное на карте
		_tGO = Instantiate( prefabSprite ) as GameObject;
		_tSR = _tGO.GetComponent<SpriteRenderer> ();
		_tSR.sprite = cardBack;
		_tGO.transform.SetParent (card.transform);
		_tGO.transform.localPosition = Vector3.zero;
		// большее значение sortingOrder, чем у других спрайтов
		_tSR.sortingOrder = 2;
		_tGO.name = "back";
		card.back = _tGO;

		// по умолчанию картинкой вверх
		card.faceUp = startFaceUp; // использовать свойство faceUp карты
	}

	// перемешивает карты в Deck.cards
	static public void Shuffle(ref List<Card> oCards) {
		// создать временный список для хранения карт в перемешанном порядке
		List<Card> tCards = new List<Card>();

		int ndx; // будет хранить индекс перемещаемой карты
		tCards = new List<Card>(); // инициализировать временный список
		// повторять, пока не будут перемещены все карты в исходном списке
		while (oCards.Count > 0) {
			// выбрать случайный индекс карты
			ndx = Random.Range(0,oCards.Count);
			// добавить эту карту во временный список
			tCards.Add (oCards[ndx]);
			// и удалить карту из исходного списка
			oCards.RemoveAt(ndx);
		}
		// заменить исходный список временным
		oCards = tCards;
		// так как oCards - это параметр-ссылка (ref), оригинальный аргумент, переданный в метод, тоже изменится
	}
}
