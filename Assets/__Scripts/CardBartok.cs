using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CBState включает состояние игры и состояния to..., описывающие движения
public enum CBState {
	toDrawpile,
	drawpile,
	toHand,
	hand,
	toTarget,
	target,
	discard,
	to,
	idle
}

public class CardBartok : Card {
	// статистические переменные совместно используются всеми экземплярами CardBartok
	static public float MOVE_DURATION = 0.5f;
	static public string MOVE_EASING = Easing.InOut;
	static public float CARD_HEIGHT = 3.5f;
	static public float CARD_WIDTH = 2f;

	[Header("Set Dynamically: CardBartok")]
	public CBState state = CBState.drawpile;

	// поля с информацией, необходимой для перемещения и поворачивания карты
	public List<Vector3> bezierPts;
	public List<Quaternion> bezierRots;
	public float timeStart, timeDuration;
	public int eventualSortOrder;
	public string eventualSortLayer;

	// по завершении перещения карты будет вызываться reportFinishTo.SendMessage()
	public GameObject reportFinishTo = null;
	[System.NonSerialized]
	public Player callbackPlayer = null;

	// MoveTo запускает перемещение карты в новое местоположение с заданным поворотом
	public void MoveTo(Vector3 ePos, Quaternion eRot) {
		// создать новые списки для интерполяции
		// траектории перемещенияы и поворота определяются двумя точками каждая
		bezierPts = new List<Vector3>();
		bezierPts.Add (transform.localPosition); // текущее местоположение
		bezierPts.Add (ePos); // новое местоположение

		bezierRots = new List<Quaternion>();
		bezierRots.Add (transform.rotation); // текущий угол поворота
		bezierRots.Add (eRot); // новый угол поворота

		if (timeStart == 0) {
			timeStart = Time.time;
		}
		// timeDuration  всегда получает одно и то же значение, но потом это можно исправить
		timeDuration = MOVE_DURATION;
		state = CBState.to;
	}

	public void MoveTo(Vector3 ePos) {
		MoveTo (ePos, Quaternion.identity);
	}
	
	void Update () {
		switch (state) {
		case CBState.toHand:
		case CBState.toTarget:
		case CBState.toDrawpile:
		case CBState.to:
			float u = (Time.time - timeStart) / timeDuration;
			float uC = Easing.Ease (u, MOVE_EASING);
			if (u < 0) {
				transform.localPosition = bezierPts [0];
				transform.rotation = bezierRots [0];
				return;
			} else if (u >= 1) {
				uC = 1;
				// перевести из состояния to... в соответствующее следующее состояние
				if (state == CBState.toHand) state = CBState.hand;
				if (state == CBState.toTarget) state = CBState.target;
				if (state == CBState.toDrawpile) state = CBState.drawpile;
				if (state == CBState.to) state = CBState.idle;

				// переместить в конечное местоположение
				transform.localPosition = bezierPts [bezierPts.Count - 1];
				transform.rotation = bezierRots [bezierPts.Count - 1]; // !!! МОЖЕТ БЫТЬ, bezierRots.Count - 1???

				// сбросить timeStart в 0, чтобы в следующий раз можно было установить текущее время
				timeStart = 0;

				if (reportFinishTo != null) {
					reportFinishTo.SendMessage ("CBCallback", this);
					reportFinishTo = null;
				} else if (callbackPlayer != null) {
					// если имеется ссылка на экземпляр Player, вызвать метод CBCallback этого экземпляра
					callbackPlayer.CBCallback(this);
					callbackPlayer = null;
				} else { // если ничего вызывать не надо, оставить как есть
				}
			} else { // нормальный режим интерполяции (0 <= u < 1)
				Vector3 pos = Utils.Bezier (uC, bezierPts);
				transform.localPosition = pos;
				Quaternion rotQ = Utils.Bezier(uC, bezierRots);
				transform.rotation = rotQ;

				if (u>0.5f) {
					SpriteRenderer sRend = spriteRenderers[0];
					if (sRend.sortingOrder != eventualSortOrder) {
						// установить конечный порядок сортировки
						SetSortOrder(eventualSortOrder);
					}
					if (sRend.sortingLayerName != eventualSortLayer) {
						// установить конечный слой сортировки
						SetSortingLayerName(eventualSortLayer);
					}
				}
			}
			break;
		}
	}

	// этот метод определяет реакцию карты на щелчок мышью
	override public void OnMouseUpAsButton() {
		// вызвать метод CardClicked объекта-одиночки Bartok
		Bartok.S.CardClicked(this);
		// также вызвать версию этого метода в базовом класса (Card.cs)
		base.OnMouseUpAsButton();
	}
}
