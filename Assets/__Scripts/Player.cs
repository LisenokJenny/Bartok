using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // подключает механизм запросов LINQ

// игрок может быть человеком или ИИ
public enum PlayerType {
	human,
	ai
}

[System.Serializable]
public class Player {
	public PlayerType type = PlayerType.ai;
	public int playerNum;
	public SlotDef handSlotDef;
	public List<CardBartok> hand; // карты в руках игрока

	// добавляет карту в руки
	public CardBartok AddCard(CardBartok eCB) {
		if (hand == null) hand = new List<CardBartok>();

		// добавить карту
		hand.Add (eCB);

		// если это человек, отсортировать карты по достоинству с помощью LINQ
		if (type == PlayerType.human) {
			CardBartok[] cards = hand.ToArray();

			// это вызов LINQ
			cards = cards.OrderBy( cd => cd.rank ).ToArray();

			hand = new List<CardBartok>(cards);

			// Примечание: LINQ выполняет операции довольно медленно (затрачивая по несколько миллисекунд),
			// так как мы делаем это один раз за раунд, это не проблема
		}

		eCB.SetSortingLayerName("10"); // перенести перемещаемую карту в верхний слой
		eCB.eventualSortLayer = handSlotDef.layerName;

		FanHand();
		return( eCB );
	}

	// удаляет карту из рук
	public CardBartok RemoveCard(CardBartok cb) {
		// если список hand пуст или не содержит карты cb, вернуть null
		if (hand == null || !hand.Contains(cb) ) return null;
		hand.Remove(cb);
		FanHand();
		return(cb);
	}

	public void FanHand() {
		// startRot - угол поворота первой карты относительно оси Z
		float startRot = 0;
		startRot = handSlotDef.rot;
		if (hand.Count > 1) {
			startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;
		}

		// переместить все карты в новые позиции
		Vector3 pos;
		float rot;
		Quaternion rotQ;
		for (int i=0; i<hand.Count; i++) {
			rot = startRot - Bartok.S.handFanDegrees*i;
			rotQ = Quaternion.Euler( 0, 0, rot );

			pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;

			pos = rotQ * pos;

			// прибавить координаты позиции руки игрока (внизу в центре веера карт)
			pos += handSlotDef.pos;
			pos.z = -0.5f*i;

			// если это не начальная раздача, начать перемещение карты немедленно
			if (Bartok.S.phase != TurnPhase.idle) {
				hand[i].timeStart = 0;
			}

			// установить локальную позицию и поворот i-й карты в руках
			hand[i].MoveTo(pos, rotQ); // сообщить карте, что она должна начать интерполяцию
			hand[i].state = CBState.toHand;
			// закончив перемещение, карта запишет в поле state значение CBState.hand

			/* <= это начало многострочного комментария
			hand[i].transform.localPosition = pos;
			hand[i].transform.rotation = rotQ;
			hand[i].state = CBState.hand;
			это конец многострочного комментария => */

			hand[i].faceUp = (type == PlayerType.human);

			// установить SortOrder карт, чтобы обеспечить правильное перекрытие
			hand[i].eventualSortOrder = i*4;
			// hand[i].SetSortOrder(i*4);
		}
	}

	//функция TakeTurn() реализует ИИ для игроков, управляемых компьютером
	public void TakeTurn() {
		Utils.tr ("Player.TakeTurn");

		// ничего не делать для игрока-человека
		if (type == PlayerType.human) return;

		Bartok.S.phase = TurnPhase.waiting;

		CardBartok cb;

		// если этим игроком управляет компьютер, нужно выбрать карту для хода
		// найти допустимые ходы
		List<CardBartok> validCards = new List<CardBartok>();
		foreach (CardBartok  tCB in hand) {
			if (Bartok.S.ValidPlay (tCB)) {
				validCards.Add (tCB);
			}
		}
		// если доступных ходов нет
		if (validCards.Count == 0) {
			// взять карту
			cb = AddCard( Bartok.S.Draw () );
			cb.callbackPlayer = this;
			return;
		}

		// итак, у нас есть одна или несколько карт, которыми можно сыграть
		// теперь нужно выбрать одну из них
		cb = validCards[ Random.Range (0,validCards.Count) ];
		RemoveCard(cb);
		Bartok.S.MoveToTarget(cb);
		cb.callbackPlayer = this;
	}

	public void CBCallback(CardBartok tCB) {
		Utils.tr ("Player.CBCallback()",tCB.name,"Player "+playerNum);
		// карта завершила перемещение, передать право хода
		Bartok.S.PassTurn();
	}
}