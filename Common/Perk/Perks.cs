
namespace WordBombServer.Common.Perk
{
    public enum GamePerk
    {
        EXTRA_HEALTH,
        EXTRA_SCORE,
        EXTRA_TIME,
        COMBO_KEEPER,
        NEXT_ONE,
        GAIN_DIAMOND,
    }

    public class PerkManager
    {
        public List<Perk> Perks = new List<Perk>();
        public PerkManager()
        {
            Perks.Add(new ExtraHealthPerk());
            Perks.Add(new ExtraTimePerk());
            Perks.Add(new ComboKeeperPerk());
            Perks.Add(new GainEmeraldPerk());
            Perks.Add(new NextOnePerk());
        }
        public Perk GetPerk(int id)
        {
            var perk = Perks.FirstOrDefault(t => t.Id == id);
            if (perk == null)
            {
                Console.WriteLine($"Error: Can't find that perk id:{id}");
            }
            return perk;
        }
    }


    public abstract class Perk
    {
        public const short DEFAULT_PERK_PRICE = 0;
        public const short GOLDEN_PERK_PRICE = 20;
        public abstract int Id { get; }
        public abstract short Price { get; }
    }

    public class ExtraHealthPerk : Perk
    {
        public override int Id => (int)GamePerk.EXTRA_HEALTH;
        public override short Price => Perk.DEFAULT_PERK_PRICE;
    }

    public class ExtraTimePerk : Perk
    {
        public override int Id => (int)GamePerk.EXTRA_TIME;
        public override short Price => Perk.DEFAULT_PERK_PRICE;
    }
    public class ComboKeeperPerk : Perk
    {
        public override int Id => (int)GamePerk.COMBO_KEEPER;
        public override short Price => Perk.DEFAULT_PERK_PRICE;
    }
    public class GainEmeraldPerk : Perk
    {
        public override int Id => (int)GamePerk.GAIN_DIAMOND;
        public override short Price => Perk.GOLDEN_PERK_PRICE;
    }

    public class NextOnePerk : Perk
    {
        public override int Id => (int)GamePerk.NEXT_ONE;
        public override short Price => Perk.GOLDEN_PERK_PRICE;
    }

}
