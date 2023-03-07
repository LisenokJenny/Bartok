using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// класс SlotDef не наследует MonoBehaviour, поэтому для него не требуется создавать отдельный файл на C#
[System.Serializable] // сделает экземпляры SlotDef видимыми в инспекторе Unity
//public class SlotDef {
//	public float x;
//	public float y;
//	public bool faceUp = false;
//	public string layerName = "Default";
//	public int layerID = 0;
//	public int id;
//	public List<int> hiddenBy = new List<int>();
//	public string type = "slot";
//	public Vector2 stagger;
//}

public class Layout : MonoBehaviour {
	public PT_XMLReader xmlr; // так же, как и Deck, имеет PT_XMLReader
	public PT_XMLHashtable xml; // используется для ускорения доступа к xml
	public Vector2 multiplier; // смещение от центра раскладки
	// ссылки SlotDef
	public List<SlotDef> slotDefs; // все экземпляры SlotDef для рядов 0-3
	public SlotDef drawPile;
	public SlotDef discardPile;
	// хранит имена всех рядов
	public string[] sortingLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

	// эта функция вызывается для чтения файла LayoutXML.xml
	public void ReadLayout(string xmlText) {
		xmlr = new PT_XMLReader();
		xmlr.Parse(xmlText); // загрузить XML
		xml = xmlr.xml["xml"][0]; // и определяется xml для ускорения доступа к XML

		// прочитать множители, определяющие расстояние между картами
		multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
		multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

		// прочитать слоты
		SlotDef tSD;
		// slotsX используется для ускорения доступа к элементам <slot>
		PT_XMLHashList slotsX = xml["slot"];

		for (int i = 0; i<slotsX.Count; i++) {
			tSD = new SlotDef(); // создать новый экземпляр SlotDef
			if (slotsX[i].HasAtt("type")) {
				// если <slot> имеет атрибут type, прочитать его
				tSD.type = slotsX[i].att ("type");
			} else {
				// иначе определить тип как "slot"; это отдельная карта в ряду
				tSD.type = "slot";
			}
			// преобразовать некоторые атрибуты в числовые значения
			tSD.x = float.Parse( slotsX[i].att("x") );
			tSD.y = float.Parse( slotsX[i].att("y") );
			tSD.layerID = int.Parse( slotsX[i].att ("layer") );
			// преобразовать номер ряда layerID в текст layerName
			tSD.layerName = sortingLayerNames[ tSD.layerID ];

			switch (tSD.type) {
			// прочитать дополнительные атрибуты, опираясь на тип слота
			case "slot":
				tSD.faceUp = (slotsX[i].att("faceup") == "1");
				tSD.id = int.Parse( slotsX[i].att("id") );
				if (slotsX[i].HasAtt("hiddenby")) {
					string[] hiding = slotsX[i].att("hiddenby").Split (',');
					foreach( string s in hiding ) {
						tSD.hiddenBy.Add( int.Parse(s) );
					}
				}
				slotDefs.Add (tSD);
				break;

			case "drawpile":
				tSD.stagger.x = float.Parse( slotsX[i].att("xstagger") );
				drawPile = tSD;
				break;
			case "discardpile":
				discardPile = tSD;
				break;
			}
		}
	}
}
