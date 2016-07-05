using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

public class Pong : PhysicsGame
{
    ExplosionSystem rajahtaa = new ExplosionSystem(LoadImage("Pulla"), 400);
    ExplosionSystem numminenRajahtaa = new ExplosionSystem(LoadImage("M_A_Numminen_2011"), 400);

    Vector nopeusYlos = new Vector(0, 650);
    Vector nopeusAlas = new Vector(0, -650);
    Vector impulssi;
    Vector impulssi2;

    PhysicsObject pallo;
    PhysicsObject maila1;
    PhysicsObject maila2;
    PhysicsObject ammus;

    PhysicsObject vasenReuna;
    PhysicsObject oikeaReuna;

    IntMeter pelaajan1Pisteet;
    IntMeter pelaajan2Pisteet;

    Boolean hasGameEnded = false;
    Boolean hasRajahtanyt = false;
    Boolean hasNumminenRajahtanyt = false;
    Boolean isTimerRunning = false;
    Boolean AlkuvalikkoKayty;

    DoubleMeter alaspainLaskuri;
    // Mhei~

    const double PALLON_MIN_NOPEUS = 500;

    public override void Begin()
    {
        if (AlkuvalikkoKayty == false)
        {
            Alkuvalikko();
        }
        LuoKentta();
        AsetaNappaimet();
        LisaaLaskurit();
        AloitaPeli();
    }
    void Alkuvalikko()
    {
        MultiSelectWindow valikko = new MultiSelectWindow("Tervetuloa peliin",
"Aloita peli", "Lopeta");
        valikko.ItemSelected += PainettiinValikonNappia;
        Add(valikko);
    }
    void PainettiinValikonNappia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                AlkuvalikkoKayty = true;
                Begin();
                break;
            case 1:
                Exit();
                break;
        }
    }
    void LuoKentta()
    {
        if (AlkuvalikkoKayty == false)
        {
            //luodaan pallo ja maila alkuvalikkoa varten, säilyvät pelin ajaksi
            pallo = new PhysicsObject(40.0, 40.0);
            pallo.Shape = Shape.Circle;
            pallo.Color = Color.White;
            pallo.X = 200.0;
            pallo.Y = 0.0;
            pallo.Restitution = 1.0;
            pallo.CanRotate = false;
            pallo.IgnoresExplosions = true;
            Add(pallo);
            AddCollisionHandler(pallo, KasittelePallonTormays);

            maila1 = LuoMaila(Level.Left + 20.0, 0.0);
            maila1.Tag = "maila";
            maila1.CollisionIgnoreGroup = 2;
            maila2 = LuoMaila(Level.Right - 20.0, 0.0);
            maila2.Tag = "maila";
            maila2.CollisionIgnoreGroup = 2;
        }
        //luodaan kenttä        
        vasenReuna = Level.CreateLeftBorder();
        vasenReuna.Restitution = 1.0;
        vasenReuna.IsVisible = false;
        vasenReuna.CollisionIgnoreGroup = 1;
        vasenReuna.Tag = "seina";
        oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Restitution = 1.0;
        oikeaReuna.IsVisible = false;
        oikeaReuna.CollisionIgnoreGroup = 1;
        oikeaReuna.Tag = "seina";
        PhysicsObject alaReuna = Level.CreateBottomBorder();
        alaReuna.Restitution = 1.0;
        alaReuna.IsVisible = false;
        alaReuna.CollisionIgnoreGroup = 1;
        alaReuna.Tag = "seina";
        PhysicsObject ylaReuna = Level.CreateTopBorder();
        ylaReuna.Restitution = 1.0;
        ylaReuna.IsVisible = false;
        ylaReuna.CollisionIgnoreGroup = 1;
        ylaReuna.Tag = "seina";

        Level.Background.Color = Color.Black;
        Camera.ZoomToLevel();

    }
    void AloitaPeli()
    {
        //liikutetaan palloa
        impulssi = new Vector(500.0, 0.0);
        pallo.Hit(impulssi);
        impulssi2 = new Vector(-500.0, 0.0);
    }
    PhysicsObject LuoMaila(double x, double y)
    {
        //luodaan maila
        PhysicsObject maila = PhysicsObject.CreateStaticObject(20.0, 100.0);
        maila.Shape = Shape.Rectangle;
        maila.X = x;
        maila.Y = y;
        maila.Restitution = 1.0;
        Add(maila);
        return maila;
    }
    void AsetaNappaimet()
    {
        //pelaajan 1 napit
        Keyboard.Listen(Key.A, ButtonState.Down, AsetaNopeus, "Pelaaja 1: Liikuta mailaa ylös", maila1, nopeusYlos);
        Keyboard.Listen(Key.A, ButtonState.Released, AsetaNopeus, null, maila1, Vector.Zero);
        Keyboard.Listen(Key.Z, ButtonState.Down, AsetaNopeus, "Pelaaja 1: Liikuta mailaa alas", maila1, nopeusAlas);
        Keyboard.Listen(Key.Z, ButtonState.Released, AsetaNopeus, null, maila1, Vector.Zero);

        Keyboard.Listen(Key.X, ButtonState.Down, AmmuPalloa1, "Pelaaja 1: Ammu");

        //pelaajan 2 napit
        Keyboard.Listen(Key.Up, ButtonState.Down, AsetaNopeus, "Pelaaja 2: Liikuta mailaa ylös", maila2, nopeusYlos);
        Keyboard.Listen(Key.Up, ButtonState.Released, AsetaNopeus, null, maila2, Vector.Zero);
        Keyboard.Listen(Key.Down, ButtonState.Down, AsetaNopeus, "Pelaaja 2: Liikuta mailaa alas", maila2, nopeusAlas);
        Keyboard.Listen(Key.Down, ButtonState.Released, AsetaNopeus, null, maila2, Vector.Zero);

        Keyboard.Listen(Key.Left, ButtonState.Down, AmmuPalloa2, "Pelaaja 2: Ammu");

        //yleiset napit
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli~?");

    }
    void AsetaNopeus(PhysicsObject maila, Vector nopeus)
    {
        //estetään mailaa menemästä reunan yli
        if ((nopeus.Y < 0) && (maila.Bottom < Level.Bottom))
        {
            maila.Velocity = Vector.Zero;
            return;
        }
        if ((nopeus.Y > 0) && (maila.Top > Level.Top))
        {
            maila.Velocity = Vector.Zero;
            return;
        }
        maila.Velocity = nopeus;
    }
    void LisaaLaskurit()
    {
        //luodaan näkyviä laskureita
        pelaajan1Pisteet = LuoPisteLaskuri(Screen.Left + 100.0, Screen.Top - 100.0);
        pelaajan2Pisteet = LuoPisteLaskuri(Screen.Right - 100.0, Screen.Top - 100.0);

    }
    IntMeter LuoPisteLaskuri(double x, double y)
    {
        IntMeter laskuri = new IntMeter(0);
        laskuri.MaxValue = 10;

        Label naytto = new Label();
        naytto.BindTo(laskuri);
        naytto.X = x;
        naytto.Y = y;
        naytto.TextColor = Color.White;
        naytto.BorderColor = Level.Background.Color;
        naytto.Color = Level.Background.Color;
        Add(naytto);

        return laskuri;
    }
    void KasittelePallonTormays(PhysicsObject pallo, PhysicsObject kohde)
    {
        if (hasGameEnded == true)
        {
            return;
        }
        if (kohde == oikeaReuna)
        {
            pelaajan1Pisteet.Value += 1;
            Rajahdys();
        }
        else if (kohde == vasenReuna)
        {
            pelaajan2Pisteet.Value += 1;
            RajahdysNumminen();
        }
        if (pelaajan1Pisteet == 10)
        {
            MessageDisplay.Add("Pelaaja 1 voittaa");
            LuoAikaLaskuri();
            Rajahdys();
        }
        else if (pelaajan2Pisteet == 10)
        {
            MessageDisplay.Add("Pelaaja 2 voittaa");
            LuoAikaLaskuri();
            RajahdysNumminen();
        }
    }
    void LuoAikaLaskuri()
    {
        if (isTimerRunning == true)
        {
            return;
        }

        alaspainLaskuri = new DoubleMeter(5);

        pallo.Velocity = Vector.Zero;
        Timer aikaLaskuri = new Timer();
        aikaLaskuri.Interval = 0.1;
        aikaLaskuri.Timeout += AikaLoppui;
        aikaLaskuri.Start();

        Label aikaNaytto = new Label();
        aikaNaytto.TextColor = Color.White;
        aikaNaytto.DecimalPlaces = 1;
        aikaNaytto.BindTo(alaspainLaskuri);
        Add(aikaNaytto);
        isTimerRunning = true;
    }
    void AikaLoppui()
    {
        alaspainLaskuri.Value -= 0.1;
        if (alaspainLaskuri.Value <= 0)
        {
            alaspainLaskuri.Stop();
            Exit();
        }
    }
    protected override void Update(Time time)
    {
        if (pallo != null && Math.Abs(pallo.Velocity.X) < PALLON_MIN_NOPEUS)
        {
            pallo.Velocity = new Vector(pallo.Velocity.X * 1.1, pallo.Velocity.Y);
        }
        base.Update(time);
    }
    void Rajahdys()
    {
        if (hasRajahtanyt == false)
        {
            hasRajahtanyt = true;
            Add(rajahtaa);
        }
        double x = 0;
        double y = 0;
        int pMaara = 400;
        rajahtaa.AddEffect(x, y, pMaara);
    }
    void RajahdysNumminen()
    {
        if (hasNumminenRajahtanyt == false)
        {
            hasNumminenRajahtanyt = true;
            Add(numminenRajahtaa);
        }
        double x = 0;
        double y = 0;
        int pMaara = 400;
        numminenRajahtaa.AddEffect(x, y, pMaara);


    }
    void AmmuPalloa1()
    {
        LuoAmmus(maila1.X + 10.0, maila1.Y, Shape.Star);
        ammus.Hit(impulssi);
    }
    void AmmuPalloa2()
    {
        LuoAmmus(maila2.X - 10.0, maila2.Y, Shape.Triangle);
        ammus.Hit(impulssi2);
    }
    void LuoAmmus(double x, double y, Shape shape)
    {
        ammus = new PhysicsObject(10.0, 10.0);
        ammus.X = x;
        ammus.Y = y;
        ammus.Shape = shape;
        ammus.CanRotate = true;
        Add(ammus);
        AddCollisionHandler(ammus, "seina", CollisionHandler.DestroyObject);
        ammus.CollisionIgnoreGroup = 2;
    }
}
