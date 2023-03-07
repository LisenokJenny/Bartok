using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// перечисление, определяющее тип переменной, которая может принимать несколько предопределенных значений
public enum eCardState {
	drawpile,
	tableau,
	target,
	discard
}

public class CardProspector : Card { // CardPropspector должен расширять Card
	[Header ("Set Dynamically: CardProspector")]
	// так используется перечисление eCardState
	public eCardState state = eCardState.drawpile;
	// hiddenBy - список других карт, не позволяющих перевернуть эту лицом вверх
	public List<CardProspector> hiddenBy = new List<CardProspector>();
	// layoutID определяет для этой карты ряд в раскладке
	public int layoutID;
	// класс SlotDef хранит информацию из элемента <slot> в LayoutXML
	public SlotDef slotDef;

	// определяет реакцию карт на щелчок мыши
	override public void OnMouseUpAsButton() {
		// вызвать метод CardClicked объекта-одиночки Prospector
		Prospector.S.CardClicked(this);
		// а также версию этого метода в базовом классе (Card.cs)
		base.OnMouseUpAsButton();
	}
}
