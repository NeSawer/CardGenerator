using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;

Console.OutputEncoding = System.Text.Encoding.UTF8;

//GeneratePatterns();
ParseRaw();

static void ParseRaw()
{
    var imgFiles = Directory.GetFiles("images");
    foreach (var img in imgFiles)
        if (img.ToLower() != img)
            File.Move(img, img.ToLower());

    Directory.CreateDirectory("heroes");
    Directory.CreateDirectory("components");
    Directory.CreateDirectory("tools");
    Directory.CreateDirectory("tests");

    var usedImages = new List<string>();

    var heroes = new List<Hero>();
    var components = new List<Component>();
    var tools = new List<Tool>();
    var tests = new List<Test>();

    CardType currentType = CardType.None;
    foreach (var rawStr in File.ReadAllLines("input.txt"))
    {
        var str = rawStr.Trim();

        if (string.IsNullOrWhiteSpace(str) ||
            str == "Контент" || str == "Карты предметов" ||
            str.StartsWith("Название ") || str.StartsWith("Имя злодея"))
            continue;

        if (str == "Карты персонажей")
        {
            currentType = CardType.Hero;
            continue;
        }
        if  (str == "Компоненты")
        {
            currentType = CardType.Component;
            continue;
        }
        if (str == "Инструменты")
        {
            currentType = CardType.Intellect;
            continue;
        }
        if (str == "Карты испытаний")
        {
            currentType = CardType.Test;
            continue;
        }

        if (str.Length < 40)
            Console.WriteLine("Line too small '{0}'", str);

        switch (currentType)
        {
            case CardType.Hero:
                heroes.Add(ParseRawHero(str));
                break;

            case CardType.Component:
                components.Add(ParseRawComponent(str));
                break;

            case CardType.Intellect:
                var tool = ParseRawTool(str);
                tools.Add(tool);
                foreach (var craft in tool.Crafts)
                {
                    var component = components.FirstOrDefault(c => c.Name == craft.component);
                    if (component is null)
                        Console.WriteLine("Component '{0}' not found.", craft.component);
                    else
                    {
                        component.Crafts.Add(tool.Name);
                        if (component.Crafts.Count > 3)
                            component.TooMuch = true;
                    }
                }
                break;

            case CardType.Test:
                tests.Add(ParseRawTest(str));
                break;

            case CardType.None:
                Console.WriteLine("Card type is none.");
                break;
        }
    }

    Console.WriteLine("Parsed: {0} heroes, {1} components, {2} tools, {3} tests.", heroes.Count, components.Count, tools.Count, tests.Count);

    foreach (var hero in heroes.Select(h => GenerateCards(h, GetImagePaths(h, usedImages))))
        foreach (var card in hero)
            card.image.Save("heroes/" + card.name + ".png");

    foreach (var component in components.Select(c => GenerateCards(c, GetImagePaths(c, usedImages))))
        foreach (var card in component)
            card.image.Save("components/" + card.name + ".png");

    foreach (var tool in tools.Select(t => GenerateCards(t, GetImagePaths(t, usedImages))))
        foreach (var card in tool)
            card.image.Save("tools/" + card.name + ".png");

    foreach (var test in tests.Select(t => GenerateCards(t, GetImagePaths(t, usedImages))))
        foreach (var card in test)
            card.image.Save("tests/" + card.name + ".png");

    Console.WriteLine("All generated.");

    var files = Directory.GetFiles("images/");
    foreach (var file in files)
    {
        if (!usedImages.Contains(Path.GetFileName(file)[..^4]))
            Console.WriteLine("Image '{0}' not used", file);
    }
}

static Hero ParseRawHero(string str)
{
    var cd = str.Split("\t").Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
    var card = new Hero
    {
        Name = FormatName(cd[0]),
        Description = cd[1],
        Info = cd[2],
        BackColor = "#c49a47"
    };
    return card;
}

static Component ParseRawComponent(string str)
{
    var cd = str.Split("\t").Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
    var card = new Component
    {
        Name = FormatName(cd[0]),
        Description = cd[1],
        Cost = int.Parse(cd[2]),
        BackColor = "#c49a47"
    };
    return card;
}

static Tool ParseRawTool(string str)
{
    var cd = str.Split("\t").Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
    var card = new Tool
    {
        Name = FormatName(cd[0]),
        Description = cd[1],
        Cost = int.Parse(cd[2]),
        Crafts = new List<(string component, int count)>(),
        BackColor = "#c49a47"
    };
    foreach (var item in cd[3].Split(',', '+').Select(i => i.Trim()).Where(i => i.Length > 0))
    {
        if (item == "-")
            continue;
        var component = FormatName(char.IsDigit(item[^1]) ? item[..^3] : item);
        var count = char.IsDigit(item[^1]) ? int.Parse(item[^1].ToString()) : 1;

        card.Crafts.Add((component, count));
    }
    return card;
}

static Test ParseRawTest(string str)
{
    var cd = str.Split("\t").Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
    var card = new Test
    {
        Name = FormatName(cd[0]),
        Description = cd[1],
        Ways = new List<(string tool, int chance)>(),
        BackColor = "#c49a47"
    };
    for (var i = 1; i < cd.Length / 2; i++)
    {
        var tool = FormatName(cd[i * 2]);
        var chance = cd[i * 2 + 1].Trim('+');

        if (chance.Length != 1 || !char.IsDigit(chance[0]))
            Console.WriteLine("Test '{0}' chance '{1}' invalid format.", cd[0], chance);
        else
            card.Ways.Add((tool, int.Parse(chance[0..1])));
    }
    return card;
}

static string FormatName(string name) => char.ToUpper(name[0]) + name[1..].ToLower();

static IEnumerable<(string name, string? path)> GetImagePaths(ICard card, List<string> usedImages)
{
    var name = card.Name.ToLower();
    if (File.Exists("images/" + name + ".png"))
    {
        usedImages.Add(name);
        yield return (card.Name, "images/" + name + ".png");

        var i = 2;
        while (File.Exists("images/" + name + ' ' + i + ".png"))
        {
            usedImages.Add(name + ' ' + i);
            yield return (card.Name + ' ' + i, "images/" + name + ' ' + i + ".png");
            i++;
        }
        yield break;
    }

    Console.WriteLine("Images for card '{0}' not found.", card.Name);
    yield return (card.Name, null);
}

static void GeneratePatterns()
{
    var card = new Card
    {
        Type = CardType.Component,
        Name = "Носок",
        Description = "В этой тюрьме носки не только для ног! В комбинации с мылом эта безобидная вещь превращается в опасное оружие.",
        Cost = 0,
        BackColor = "#c29641",
        Info = "Носок с мылом, Носок без мыла"
    };
    var card1 = new Card
    {
        Type = CardType.Intellect,
        Name = "Носок с мылом",
        Description = "В этой тюрьме носки не только для ног! В комбинации с мылом эта безобидная вещь превращается в опасное оружие.",
        Cost = 1,
        BackColor = "#c29641",
        Info = "Носок + Мыло"
    };
    var card2 = new Card
    {
        Type = CardType.Test,
        Name = "Высокая наружная стена",
        Description = "Эту стену можно перелезть, но она слишком высокая. Может лучше попробовать прокопать под ней или пробить ее?",
        Cost = 1,
        BackColor = "#888888",
        Info = "Кирка,3+;Лопата,4+;Лестница,5+"
    };

    var card3 = new Card
    {
        Type = CardType.Hero,
        Name = "Гримлик Трофезуб",
        Description = "Гоблин-коллекционер, крадущий драгоценные сокровища и редкие предметы из самых защищенных хранилищ королевства.",
        BackColor = "#888888",
        Info = "Размер тайника уменьшен на 1 единицу, размер руки увеличен на 3 единицы."
    };

    var bm = new Bitmap(1230, 450);
    var g = Graphics.FromImage(bm);
    GenerateCard(card, null).Save("patterns/pattern1.png");
    GenerateCard(card1, null).Save("patterns/pattern2.png");
    GenerateCard(card2, null).Save("patterns/pattern3.png");
    GenerateCard(card3, null).Save("patterns/pattern4.png");
    g.DrawImage(GenerateCard(card, null), 0, 0);
    g.DrawImage(GenerateCard(card1, null), 310, 0);
    g.DrawImage(GenerateCard(card2, null), 620, 0);
    g.DrawImage(GenerateCard(card3, null), 930, 0);
    bm.Save("patterns/combinedPattern.png");
    Thread.Sleep(100);
    Process.Start("explorer.exe", "patterns/combinedPattern.png");
}

static IEnumerable<(string name, Image image)> GenerateCards(ICard card, IEnumerable<(string name, string? path)> imagePaths, bool showStat = false)
{
    return imagePaths.Select(p => (p.name, GenerateCard(card, p.path, showStat)));
}

static Image GenerateCard(ICard card, string? imagePath, bool showStat = false)
{
    var bm = new Bitmap(300, 450);

    Graphics g = Graphics.FromImage(bm);
    
    g.FillPath(new SolidBrush(Col(card.BackColor)), RoundedRect(new Rectangle(0, 0, 300, 200), 40, isBot: false));

    if (imagePath is not null)
        g.DrawImage(Image.FromFile(imagePath), 60, 0, 180, 180);

    var nc = card.Type switch
    {
        CardType.Test => Col2(("#ff8866", "#f52211")),
        CardType.Component => Col2(("#eeee0d", "#efa043")),
        CardType.Strength => Col2(("#eeee0d", "#efa043")),
        CardType.Dodge => Col2(("#eeee0d", "#efa043")),
        CardType.Intellect => Col2(("#eeee0d", "#efa043")),
        CardType.Hero => Col2(("#ff8866", "#f52211"))
    };

    g.FillRectangle(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), nc.f, nc.s, 0f), new Rectangle(0, 200, 200, 50));
    g.FillRectangle(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), nc.s, nc.f, 0f), new Rectangle(200, 200, 100, 50));

    var descColor = Col2(card.Type switch
    {
        CardType.Test => ("#f5fb81", "#e1a748"),
        CardType.Component => ("#f5fb81", "#e1a748"),
        CardType.Strength => ("#f5fb81", "#e1a748"),
        CardType.Dodge => ("#f5fb81", "#e1a748"),
        CardType.Intellect => ("#f5fb81", "#e1a748"),
        CardType.Hero => ("#f5fb81", "#e1a748")
    });
    g.FillRectangle(new LinearGradientBrush(new Rectangle(0, 250, 300, 150), descColor.f, descColor.s, 90f), new Rectangle(0, 250, 300, 150));

    var downColor = Col("#8d8355"); Col(card.Type switch
    {
        CardType.Test => "#bdb183",
        CardType.Component => "#8d8355",
        CardType.Strength => "#8d8355",
        CardType.Dodge => "#8d8355",
        CardType.Intellect => "#8d8355",
        CardType.Hero => "#bdb183"
    });

    //if (card.Type != CardType.Test && card.Type != CardType.Hero)
    //    g.FillPath(new SolidBrush(downColor), RoundedRect(new Rectangle(0, 400, 300, 50), 40, isTop: false));
    //else
        g.FillPath(new SolidBrush(downColor), RoundedRect(new Rectangle(0, 365, 300, 85), 40, isTop: false));

    if (card.Type != CardType.Test && card.Type != CardType.Hero)
        g.FillRectangle(new SolidBrush(Col("#666666")), 170, 170, 130, 30);
    else
        g.FillRectangle(new SolidBrush(Col("#666666")), 0, 170, 130, 30);

    var lc = Col2(card.Type switch
    {
        CardType.Test => ("#c0c0e0", "#c0c0c7"),
        CardType.Component => ("#e6af5b", "#efdf67"),
        CardType.Strength => ("#e6af5b", "#efdf67"),
        CardType.Dodge => ("#e6af5b", "#efdf67"),
        CardType.Intellect => ("#e6af5b", "#efdf67"),
        CardType.Hero => ("#e6af5b", "#efdf67")
    });

    g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.f, lc.s, 0f), 5), 0, 200, 200, 200);
    g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.s, lc.f, 0f), 5), 200, 200, 300, 200);

    g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.f, lc.s, 0f), 5), 0, 250, 200, 250);
    g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.s, lc.f, 0f), 5), 200, 250, 300, 250);

    //if (card.Type != CardType.Test && card.Type != CardType.Hero)
    //{
    //    g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.f, lc.s, 0f), 5), 0, 400, 200, 400);
    //    g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.s, lc.f, 0f), 5), 200, 400, 300, 400);
    //}
    //else
    {
        g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.f, lc.s, 0f), 5), 0, 365, 200, 365);
        g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.s, lc.f, 0f), 5), 200, 365, 300, 365);
    }

    if (card.Type != CardType.Test && card.Type != CardType.Hero)
    {
        g.DrawLine(new Pen(lc.s, 5), 170, 200, 170, 175);
        g.DrawArc(new Pen(lc.s, 5), 170, 170, 10, 10, 180, 90);
        g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(170, 0, 200, 50), lc.s, lc.f, 0f), 5), 175, 170, 300, 170);
    }
    else
    {
        g.DrawLine(new Pen(lc.s, 5), 130, 200, 130, 175);
        g.DrawArc(new Pen(lc.s, 5), 120, 170, 10, 10, 270, 90);
        g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(170, 0, 200, 50), lc.f, lc.s, 0f), 5), 126, 170, 0, 170);
    }

    if (card.Type != CardType.Test && card.Type != CardType.Hero)
        g.DrawImage(Image.FromFile("money.png"), 240, 390, 50, 50);
    
    var families = new PrivateFontCollection();
    families.AddFontFile("font.ttf");
    var family = families.Families[0];

    var type = card.Type switch
    {
        CardType.Test => "Испытание",
        CardType.Component => "Компонент",
        CardType.Strength => showStat ? "Сила" : "Инструмент",
        CardType.Dodge => showStat ? "Ловкость" : "Инструмент",
        CardType.Intellect => showStat ? "Интеллект" : "Инструмент",
        CardType.Hero => "Злодей"
    };

    if (card.Type == CardType.Test || card.Type == CardType.Hero)
        g.DrawString(type, new Font(family, 10, FontStyle.Bold), Brushes.Red, new Rectangle(0, 170, 130, 30), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
    else if (card.Type == CardType.Component)
        g.DrawString(type, new Font(family, 10, FontStyle.Bold), Brushes.Yellow, new Rectangle(170, 170, 130, 30), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
    else
        g.DrawString(type, new Font(family, 10, FontStyle.Bold), Brushes.LightGreen, new Rectangle(170, 170, 130, 30), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

    g.DrawString(card.Name, new Font(family, 15, FontStyle.Bold), Brushes.Black, new Rectangle(10, 200, 280, 50), new StringFormat { LineAlignment = StringAlignment.Center});

    g.DrawString(card.Description, new Font(family, 11), Brushes.Black, new Rectangle(5, 260, 290, 190), new StringFormat { Alignment = StringAlignment.Center });

    if (card.Type == CardType.Test)
    {
        g.DrawString("Инструмент:", new Font(family, 9, FontStyle.Bold), Brushes.Yellow, new Rectangle(70, 370, 295, 25));
        g.DrawString("Шанс:", new Font(family, 9, FontStyle.Bold), Brushes.Blue, new Rectangle(200, 370, 280, 25));

        var i = 0;
        foreach (var item in card.Info.Split(";"))
        {
            var x = item.IndexOf(',');
            g.DrawString(item[..(x)], new Font(family, 8, FontStyle.Bold), Brushes.WhiteSmoke, new Rectangle(60, 390 + 20 * i, 115, 30), new StringFormat { Alignment = StringAlignment.Far });
            g.DrawString(item[(x + 1)..], new Font(family, 8, FontStyle.Bold), Brushes.Black, new Rectangle(205, 390 + 20 * i, 280, 30));
            i++;
        }
    }
    else if (card.Type == CardType.Hero)
    {
        g.DrawString("Способность:", new Font(family, 11, FontStyle.Bold), Brushes.Black, new Rectangle(15, 370, 280, 25));
        g.DrawString(card.Info, new Font(family, 10, FontStyle.Bold), Brushes.Yellow, new Rectangle(10, 391, 280, 55), new StringFormat { Alignment = StringAlignment.Center});
    }
    else if (card.Type == CardType.Component)
    {
        if (string.IsNullOrWhiteSpace(card.Info))
            g.DrawString("Нужен для крафта множества предметов", new Font(family, 9, FontStyle.Bold), Brushes.Black, new Rectangle(15, 370, 250, 25));
        else
        {
            g.DrawString("Нужен для крафта:", new Font(family, 9, FontStyle.Bold), Brushes.Black, new Rectangle(15, 370, 280, 25));
            var i = 0;
            foreach (var info in card.Info?.Split('+', ',').Select(s => s.Trim()) ?? Enumerable.Empty<string>())
            {
                g.DrawString("·" + info, new Font(family, 8, FontStyle.Bold), Brushes.Yellow, new Rectangle(25, 390 + 20 * i, 280, 30));
                i++;
            }
        }
    }
    else
    {
        if (string.IsNullOrWhiteSpace(card.Info))
            g.DrawString("Не крафтится", new Font(family, 9, FontStyle.Bold), Brushes.Black, new Rectangle(15, 370, 280, 25));
        else
        {
            g.DrawString("Крафтится из:", new Font(family, 9, FontStyle.Bold), Brushes.Black, new Rectangle(15, 370, 280, 25));
            var i = 0;
            foreach (var info in card.Info?.Split('+', ',').Select(s => s.Trim()) ?? Enumerable.Empty<string>())
            {
                g.DrawString("·" + info, new Font(family, 8, FontStyle.Bold), Brushes.Yellow, new Rectangle(25, 390 + 20 * i, 280, 30));
                i++;
            }
        }
    }

    if (card.Type != CardType.Test && card.Type != CardType.Hero)
    g.DrawString(card.Cost.ToString(), new Font(family, 16, FontStyle.Bold), Brushes.Black, new Rectangle(240, 392, 50, 50), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

    return bm;
}

static Color Col(string hex)
{
    return ColorTranslator.FromHtml(hex);
}

static (Color f, Color s) Col2((string f, string s) c)
{
    return (Col(c.f), Col(c.s));
}

static GraphicsPath RoundedRect(Rectangle bounds, int radius, bool isTop = true, bool isBot = true)
{
    int diameter = radius * 2;
    Size size = new Size(diameter, diameter);
    Rectangle arc = new Rectangle(bounds.Location, size);
    GraphicsPath path = new GraphicsPath();

    if (radius == 0)
    {
        path.AddRectangle(bounds);
        return path;
    }

    // top left arc
    if (isTop)
        path.AddArc(arc, 180, 90);
    else
        path.AddLine(bounds.Left, bounds.Top, bounds.Right, bounds.Top);

    // top right arc  
    arc.X = bounds.Right - diameter;
    if (isTop)
        path.AddArc(arc, 270, 90);

    // bottom right arc  
    arc.Y = bounds.Bottom - diameter;

    if (isBot)
        path.AddArc(arc, 0, 90);
    else
        path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);

    // bottom left arc 
    arc.X = bounds.Left;
    if (isBot)
        path.AddArc(arc, 90, 90);

    path.CloseFigure();
    return path;
}

class Hero : ICard
{
    public CardType Type => CardType.Hero;
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Info { get; set; }
    public string BackColor { get; set; }
    public int Cost => -1;
}

class Component : ICard
{
    public CardType Type => CardType.Component;
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Info => TooMuch ? "Множество крафтов" : string.Join(',', Crafts);
    public string BackColor { get; set; }
    public int Cost { get; set; }

    public bool TooMuch { get; set; }
    public List<string> Crafts { get; } = new List<string>();
}

class Tool : ICard
{
    public CardType Type => CardType.Intellect;
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Info => string.Join('+', Crafts.Select(c => c.count == 1 ? c.component : $"{c.component} x{c.count}"));
    public string BackColor { get; set; }
    public int Cost { get; set; }

    public List<(string component, int count)> Crafts { get; set; }
}

class Test : ICard
{
    public CardType Type => CardType.Test;
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Info => string.Join(';', Ways.Select(w => $"{w.tool},{w.chance}+"));
    public string BackColor { get; set; }
    public int Cost => -1;

    public List<(string tool, int chance)> Ways { get; set; }
}

class Card : ICard
{
    public CardType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Info { get; set; }
    public string BackColor { get; set; }
    public int Cost { get; set; }

}

internal interface ICard
{
    string BackColor { get; }
    int Cost { get; }
    string Description { get; }
    string? Info { get; }
    string Name { get; }
    CardType Type { get; }
}

enum CardType
{
    None,
    Component,
    Strength,
    Dodge,
    Intellect,
    Test,
    Hero
}
