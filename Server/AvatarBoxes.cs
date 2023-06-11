

namespace WordBombServer.Server
{
    public abstract class AvatarBox
    {
        public abstract short Price { get; }
        public abstract short Id { get; }
        public abstract string Name { get; }
        public List<string> Contents = new List<string>();
        public void AddOrdered(string tag, int amount)
        {
            for (int i = 0; i <= amount; i++)
            {
                Contents.Add($"{tag}_{i}");
            }
        }
    }

    public class AnimalBox : AvatarBox
    {
        public override short Price => 10;

        public override string Name => "Animals";

        public override short Id => 0;

        public AnimalBox()
        {
            AddOrdered("animals", 17);
        }
    }

    public class AnimalBox2 : AvatarBox
    {
        public override string Name => "Animals";
        public override short Price => 10;

        public override short Id => 1;

        public AnimalBox2()
        {
            for (int i = 19; i <= 39; i++)
            {
                Contents.Add($"animals_{i}");
            }
        }
    }

    public class VegetablesBox : AvatarBox
    {
        public override string Name => "Vegetables";
        public override short Price => 20;
        public override short Id => 2;

        public VegetablesBox()
        {
            AddOrdered("carrots", 17);
        }
    }

    public class GreekGodsBox : AvatarBox
    {
        public override string Name => "Greek Gods";
        public override short Price => 20;

        public override short Id => 3;

        public GreekGodsBox()
        {
            AddOrdered("god_yunans", 18);
        }
    }

    public class JobsBox : AvatarBox
    {
        public override string Name => "Jobs";

        public override short Price => 20;

        public override short Id => 4;

        public JobsBox()
        {
            AddOrdered("jobs", 17);
        }
    }
    public class JobsBox2 : AvatarBox
    {
        public override string Name => "Jobs";
        public override short Price => 20;

        public override short Id => 5;

        public JobsBox2()
        {
            for (int i = 19; i <= 39; i++)
            {
                Contents.Add($"jobs_{i}");
            }
        }
    }
    public class VarientsBox : AvatarBox
    {
        public override string Name => "Varients";
        public override short Price => 10;

        public override short Id => 6;

        public VarientsBox()
        {
            AddOrdered("characters", 14);
        }
    }

    public class AvatarBoxes
    {
        public List<AvatarBox> Avatars = new List<AvatarBox>();
        public AvatarBoxes()
        {
            Avatars.Add(new AnimalBox());
            Avatars.Add(new AnimalBox2());
            Avatars.Add(new VegetablesBox());
            Avatars.Add(new GreekGodsBox());
            Avatars.Add(new JobsBox());
            Avatars.Add(new JobsBox2());
            Avatars.Add(new VarientsBox());
        }
    }
}
