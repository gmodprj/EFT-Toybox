namespace GogglesWheel
{
    /// <summary>헬멧에 달린 여닫이의 종류. 순서와 기본 대상 판단에 쓴다.</summary>
    public enum ToggleKind
    {
        NightVision,
        Thermal,
        FaceShield,
        HelmetVisor,
        Other,
    }

    /// <summary>
    /// 여닫이 하나. 게임 타입을 들고 있지 않다 — 실제 부품은 <see cref="Id"/> 로 다시 찾는다.
    /// 그래야 고르는 판정을 EFT 없이 시험할 수 있다.
    /// </summary>
    public sealed class ToggleTarget
    {
        public string Id { get; }
        public ToggleKind Kind { get; }
        public string Name { get; }

        public ToggleTarget(string id, ToggleKind kind, string name)
        {
            Id = id;
            Kind = kind;
            Name = name;
        }
    }
}
