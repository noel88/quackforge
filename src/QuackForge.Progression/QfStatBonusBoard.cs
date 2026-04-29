namespace QuackForge.Progression
{
    // 패치들 사이에서 공유되는 현재 보너스 값 게시판.
    // StatModifierBinder 가 매 프레임 갱신, DamageActionPatch 등이 읽음.
    //
    // 왜 별도 클래스: Modifier 객체에서 직접 값 읽기는 가능하지만 hot path
    // (DamageAction.OnTriggeredPositive) 에서는 단순 static read 가 빠르고
    // 안전 (binder 가 unbound 상태일 때도 0 으로 안전).
    public static class QfStatBonusBoard
    {
        // VIT 분배로 추가된 MaxHealth 보너스. percent-damage debuff 가 사용하는
        // MaxHealth 에서 빼서 부작용 방지 (Approach D).
        public static float VitMaxHpBonus { get; internal set; }
    }
}
