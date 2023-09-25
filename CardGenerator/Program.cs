using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

var card = new Card
{
    Type = CardType.Component,
    Name = "Носок",
    Description = "В этой тюрьме носки не только для ног! В комбинации с мылом эта безобидная вещь превращается в опасное оружие.",
    Cost = 0,
    BackColor = "#c29641",
    Craft = "Носок, Мыло"
};
var card1 = new Card
{
    Type = CardType.Intellect,
    Name = "Носок с мылом",
    Description = "В этой тюрьме носки не только для ног! В комбинации с мылом эта безобидная вещь превращается в опасное оружие.",
    Cost = 1,
    BackColor = "#7777aa",
    Craft = "Носок, Мыло"
};
var card2 = new Card
{
    Type = CardType.Test,
    Name = "Высокая наружная стена",
    Description = "Эту стену можно перелезть, но она слишком высокая. Может лучше попробовать прокопать под ней или пробить ее?",
    Cost = 1,
    BackColor = "#bbbbbb",
    Craft = "Кирка-3,Лопата-4,Лестница-5"
};

var card2_ = new Card
{
    Type = CardType.Test,
    Name = "Прочная наружная стена",
    Description = "На вид стена укреплена, вряд ли получится ее пробить киркой, но она достаточно низкая, чтобы ее перелезть!",
    Cost = 1,
    BackColor = "#bbbbbb",
    Craft = "Лестница-3,Лопата-4,Кирка-5"
};

var card3 = new Card
{
    Type = CardType.Hero,
    Name = "Кто-то",
    Description = "На вид стена укреплена, вряд ли получится ее пробить киркой, но она достаточно низкая, чтобы ее перелезть!",
    BackColor = "#bbbbbb",
    Craft = "Получает баф такой-то зачем-то."
};

var bm = new Bitmap(1230, 450);
var g = Graphics.FromImage(bm);
g.DrawImage(GenerateCard(card), 0, 0);
g.DrawImage(GenerateCard(card1), 310, 0);
g.DrawImage(GenerateCard(card2), 620, 0);
g.DrawImage(GenerateCard(card3), 930, 0);
bm.Save("test.png");
Thread.Sleep(100);
Process.Start("explorer.exe", "test.png");

static Image GenerateCard(Card card, bool showStat = false)
{
    var bm = new Bitmap(300, 450);

    Graphics g = Graphics.FromImage(bm);
    
    g.FillPath(new SolidBrush(Col(card.BackColor)), RoundedRect(new Rectangle(0, 0, 300, 200), 40, isBot: false));

    if (File.Exists(card.Name + ".png"))
        g.DrawImage(Image.FromFile(card.Name + ".png"), 60, -10, 180, 180);
    else
        Console.WriteLine("File '{0}' not found.", card.Name + ".png");

    var nc = card.Type switch
    {
        CardType.Test => Col2(("#ff8866", "#ff4433")),
        CardType.Component => Col2(("#eeee0d", "#efa043")),
        CardType.Intellect => (f: Color.GreenYellow, s: Color.YellowGreen)
    };

    g.FillRectangle(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), nc.f, nc.s, 0f), new Rectangle(0, 200, 200, 50));
    g.FillRectangle(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), nc.s, nc.f, 0f), new Rectangle(200, 200, 100, 50));

    var descColor = Col(card.Type switch
    {
        CardType.Test => "#bdb183",
        CardType.Component => "#f4f4cd",
        CardType.Intellect => "#f4f4cd"
    });
    g.FillRectangle(new SolidBrush(descColor), new Rectangle(0, 250, 300, 150));

    var downColor = Col(card.Type switch
    {
        CardType.Test => "#bdb183",
        CardType.Component => "#ada375",
        CardType.Intellect => "#ada375"
    });
    g.FillPath(new SolidBrush(downColor), RoundedRect(new Rectangle(0, 400, 300, 50), 40, isTop: false));

    if (card.Type != CardType.Test)
        g.FillRectangle(Brushes.DarkGray, 170, 170, 130, 30);
    else
        g.FillRectangle(new SolidBrush(Col("#666666")), 0, 170, 130, 30);

    var lc = Col2(card.Type switch
    {
        CardType.Test => ("#d0d0d0", "#d0d0d0"),
        CardType.Component => ("#f6bf6b", "#ffef77"),
        CardType.Intellect => ("#f6bf6b", "#ffef77")
    });

    g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.f, lc.s, 0f), 5), 0, 200, 200, 200);
    g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.s, lc.f, 0f), 5), 200, 200, 300, 200);

    g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.f, lc.s, 0f), 5), 0, 250, 200, 250);
    g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.s, lc.f, 0f), 5), 200, 250, 300, 250);

    if (card.Type != CardType.Test)
    {
        g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.f, lc.s, 0f), 5), 0, 400, 200, 400);
        g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.s, lc.f, 0f), 5), 200, 400, 300, 400);
    }
    else
    {
        g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.f, lc.s, 0f), 5), 0, 345, 200, 345);
        g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(0, 0, 200, 50), lc.s, lc.f, 0f), 5), 200, 345, 300, 345);
    }

    if (card.Type != CardType.Test)
    {
        g.DrawLine(new Pen(lc.s, 5), 170, 200, 170, 175);
        g.DrawArc(new Pen(lc.s, 5), 170, 170, 10, 10, 180, 90);
        g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(170, 0, 200, 50), lc.s, lc.f, 0f), 5), 175, 170, 300, 170);
    }
    else
    {
        g.DrawLine(new Pen(lc.s, 5), 130, 200, 130, 175);
        g.DrawArc(new Pen(lc.s, 5), 120, 170, 10, 10, 270, 90);
        g.DrawLine(new Pen(new LinearGradientBrush(new Rectangle(170, 0, 200, 50), lc.s, lc.f, 0f), 5), 125, 170, 0, 170);
    }

    if (card.Type != CardType.Test)
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

    if (card.Type != CardType.Test)
        g.DrawString(type, new Font(family, 10, FontStyle.Bold), Brushes.Yellow, 175, 180);
    else
        g.DrawString(type, new Font(family, 10, FontStyle.Bold), Brushes.Red, 10, 180);

    g.DrawString(card.Name, new Font(family, 15, FontStyle.Bold), Brushes.Black, new Rectangle(10, 200, 280, 50), new StringFormat { LineAlignment = StringAlignment.Center});

    g.DrawString(card.Description, new Font(family, 11), Brushes.Black, new Rectangle(5, 260, 290, 190), new StringFormat { Alignment = StringAlignment.Center });

    if (card.Type != CardType.Component && card.Type != CardType.Test)
    {
        g.DrawString("Крафт из:", new Font(family, 12, FontStyle.Bold), Brushes.Black, new Rectangle(15, 405, 280, 25));
        g.DrawString(card.Craft, new Font(family, 8, FontStyle.Bold), Brushes.Yellow, new Rectangle(25, 426, 280, 30));
    }
    else if (card.Type == CardType.Test)
    {
        g.DrawString("Инструмент:", new Font(family, 9, FontStyle.Bold), Brushes.Yellow, new Rectangle(55, 350, 280, 25));
        g.DrawString("Шанс:", new Font(family, 9, FontStyle.Bold), Brushes.Blue, new Rectangle(200, 350, 280, 25));

        var i = 0;
        foreach (var item in card.Craft.Split(","))
        {
            var x = item.IndexOf('-');
            g.DrawString(item[..(x)], new Font(family, 8, FontStyle.Bold), Brushes.WhiteSmoke, new Rectangle(60, 370 + 20 * i, 100, 30), new StringFormat { Alignment = StringAlignment.Far});
            g.DrawString(item[(x + 1)..] + '+', new Font(family, 8, FontStyle.Bold), Brushes.Black, new Rectangle(205, 370 + 20 * i, 280, 30));
            i++;
        }
        //g.DrawString(card.Craft, new Font(family, 8, FontStyle.Bold), Brushes.Yellow, new Rectangle(25, 426, 280, 30));
    }

    if (card.Type != CardType.Test)
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

class Card
{
    public CardType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Craft { get; set; }
    public string BackColor { get; set; }
    public int Cost { get; set; }

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
