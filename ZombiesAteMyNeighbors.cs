using System;
using System.Collections.Generic;
using System.Linq;
using CrowdControl.Common;
using JetBrains.Annotations;


namespace CrowdControl.Games.Packs
{
    [UsedImplicitly]
    public class ZombiesAteMyNeighbors : SNESEffectPack
    {

        public ZombiesAteMyNeighbors([NotNull] IPlayer player, [NotNull] Func<CrowdControlBlock, bool> responseHandler, [NotNull] Action<object> statusUpdateHandler) : base(player, responseHandler, statusUpdateHandler) { }


        private const uint ADDR_GAME_STATE = 0x7E000E; //2 = playing
        private const uint ADDR_LOCKED     = 0x7E0D1A; //80 set = locked
        private const uint ADDR_PAUSED     = 0x7E1142; //80 = playing


        private const uint ADDR_HEALTH = 0x7E1CB8;
        private const uint ADDR_LIVES  = 0x7E1D4C;

        private const uint ADDR_HEALTH_VIS = 0x7E6036;
        private const uint ADDR_ITEMS_VIS  = 0x7E6042;

        private const uint ADDR_PLAYER_FUNC  = 0x7E0128;
        private const uint ADDR_PLAYER_FUNC2 = 0x7E02A8;

        private const uint ADDR_MONSTER_TIMER  = 0x7E0156;
        private const uint ADDR_MONSTER_TIMER2 = 0x7E02D6;

        private const uint ADDR_SCORE  = 0x7E1E72;
        private const uint ADDR_SCORE2 = 0x7E1E76;

        private const uint ADDR_HISCORE = 0x7E1EA0;

        private const uint ADDR_ACTIVE  = 0x7E0D25;
        private const uint ADDR_ACTIVE2 = 0x7E0DB5;

        private const uint ADDR_EQUIPPED_WEAP  = 0x7E1CBC;
        private const uint ADDR_EQUIPPED_WEAP2 = 0x7E1CBE;
        private const uint ADDR_EQUIPPED_ITEM  = 0x7E1CC0;
        private const uint ADDR_EQUIPPED_ITEM2 = 0x7E1CC2;

        private const uint ADDR_NPCS_LEFT  = 0x7E1D52;
        private const uint ADDR_NPCS_SAVED = 0x7E1F9C;

        private const uint ADDR_PLAYER1 = 0x7E1E84;
        private const uint ADDR_PLAYER2 = 0x7E1E86;

        private const uint ADDR_PLAYER1B = 0x7E010C;
        private const uint ADDR_PLAYER2B = 0x7E028C;
        //private const uint ADDR_PLAYER1_C = 0x7E010C;
        //private const uint ADDR_PLAYER1_D = 0x7E073E;
        //private const uint ADDR_PLAYER1_E = 0x7E083E;
        //private const uint ADDR_PLAYER1_F = 0x7E0CBE;

        private const uint ADDR_AMMO_SQUIRT     = 0x7E1CCC;
        private const uint ADDR_AMMO_EXTINGUISH = 0x7E1CCE;
        private const uint ADDR_AMMO_RAYGUN     = 0x7E1CD0;
        private const uint ADDR_AMMO_WEEDWHACK  = 0x7E1CD2;
        private const uint ADDR_AMMO_CROSS      = 0x7E1CD4;
        private const uint ADDR_AMMO_BAZOOKA    = 0x7E1CD6;
        private const uint ADDR_AMMO_SODA       = 0x7E1CD8;
        private const uint ADDR_AMMO_TOMATO     = 0x7E1CDA;
        private const uint ADDR_AMMO_ICEPOP     = 0x7E1CDC;
        private const uint ADDR_AMMO_BANANA     = 0x7E1CDE;
        private const uint ADDR_AMMO_DISH       = 0x7E1CE0;
        private const uint ADDR_AMMO_SILVERWARE = 0x7E1CE2;
        private const uint ADDR_AMMO_FOOTBALL   = 0x7E1CE4;
        private const uint ADDR_AMMO_FLAMETHROW = 0x7E1CE6;

        private const uint ADDR_ITEM_KEYS       = 0x7E1D0C;
        private const uint ADDR_ITEM_SNEAKERS   = 0x7E1D0E;
        private const uint ADDR_ITEM_MONPOTIONS = 0x7E1D10;
        private const uint ADDR_ITEM_INVPOTIONS = 0x7E1D12;
        private const uint ADDR_ITEM_MYSPOTIONS = 0x7E1D14;

        private const uint ADDR_ITEM_MEDKITS    = 0x7E1D1A;
        private const uint ADDR_ITEM_PANDORABOX = 0x7E1D1C;
        private const uint ADDR_ITEM_SKULLKEYS  = 0x7E1D1E;

        private const uint ADDR_PACIFY          = 0x7E014C;
        private const uint ADDR_PACIFY2         = 0x7E02CC;

        private const uint ADDR_STATE           = 0x7E0170;
        private const uint ADDR_STATE2          = 0x7E02F0;

        private const uint ADDR_MONSTER        = 0x7E016A;
        private const uint ADDR_MONSTER2       = 0x7E02EA;

        private const uint ADDR_SFX_TRIGGER    = 0x7FFFF0;
        private const uint ADDR_SFX_ID         = 0x7FFFF2;
        private const uint ADDR_AMMO_TRIGGER   = 0x7FFFF4;
        private const uint ADDR_DPAD_TRIGGER   = 0x7FFFF6;
        private const uint ADDR_DPAD2_TRIGGER  = 0x7FFFF8;

        private const uint ADDR_SHOW_MAP       = 0x7E1F98;
        private const uint ADDR_SHOW_MAP2      = 0x7E1F9A;

        private const uint ADDR_XPOS           = 0x7E0130;
        private const uint ADDR_XPOSB          = 0x7E0134;
        private const uint ADDR_YPOS           = 0x7E0132;
        private const uint ADDR_YPOSB          = 0x7E0136;

        private const uint ADDR_XPOS2          = 0x7E02B0;
        private const uint ADDR_XPOSB2         = 0x7E02B4;
        private const uint ADDR_YPOS2          = 0x7E02B2;
        private const uint ADDR_YPOSB2         = 0x7E02B6;

        private byte oldhealth;
        private byte oldhealth2;

        private Dictionary<string, (string text, int id, int amount, int max)> _weapons = new Dictionary<string, (string, int, int, int)>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"squirt",      ("Squirt Gun",      0,  99,  999)},
            {"extinguish",  ("Extinguisher",    1,  99,  999)},
            {"raygun",      ("Ray Gun",         2,  40,  240)},
            {"weed",        ("Weed Whacker",    3,  150, 999)},
            {"cross",       ("Holy Cross",      4,  30,  999)},                                                                          
            {"bazooka",     ("Bazooka",         5,  5,   999)},
            {"soda",        ("Soda",            6,  20,  999)},
            {"tomato",      ("Tomato",          7,  20,  999)},
            {"icepop",      ("Ice Pop",         8,  20,  999)},
            {"banana",      ("Banana",          9,  20,  999)},
            {"dih",         ("Dish",            10, 20,  999)},
            {"silverware",  ("Silverware",      11, 20,  999)},
            {"football",    ("Football",        12, 20,  100)},
            {"flame",       ("Flamethrower",    13, 100, 400)},
        };

        private Dictionary<string, (string text, byte id)> _items = new Dictionary<string, (string, byte)>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"keys",        ("Key",            0)},
            {"sneakers",    ("Sneaker",        1)},
            {"monster",     ("Monster Potion", 2)},
            {"invin",       ("Invincibility Potion",   3)},
            {"mystery",     ("Mystery Potion", 4)},                                                                          
            {"medkits",     ("Medkit",         7)},
            {"pandora",     ("Pandora's Box",  8)},
            {"skull",       ("Skull Key",      9)},
        };

        public override List<Effect> Effects
        {
            get
            {
                List<Effect> effects = new List<Effect>
                {
                    new Effect("Add Ammo", "addammo", ItemKind.Folder),
                    new Effect("Take Ammo", "takeammo", ItemKind.Folder),
                    new Effect("Add Ammo P2", "addammo2", ItemKind.Folder),
                    new Effect("Take Ammo P2", "takeammo2", ItemKind.Folder),

                    new Effect("Add item", "additem", ItemKind.Folder),
                    new Effect("Take item", "takeitem", ItemKind.Folder),
                    new Effect("Add item P2", "additem2", ItemKind.Folder),
                    new Effect("Take item P2", "takeitem2", ItemKind.Folder),

                    new Effect("Heal", "heal"),
                    new Effect("Hurt", "hurt"),
                    new Effect("Full Heal", "fullheal"),
                    new Effect("Kill", "kill"),

                    new Effect("Squash", "squash"),
                    new Effect("Squash P2", "squash2"),

                    new Effect("Heal P2", "heal2"),
                    new Effect("Hurt P2", "hurt2"),
                    new Effect("Full Heal P2", "fullheal2"),
                    new Effect("Kill P2", "kill2"),

                    new Effect("Add Life", "oneup"),
                    new Effect("Take Life", "onedown"),
                    new Effect("Add Life P2", "oneup2"),
                    new Effect("Take Life P2", "onedown2"),

                    new Effect("Swap Characters", "swap"),

                    new Effect("Spawn Door", "door"),

                    new Effect("Grant Bonus Victim", "addvic"),
                    new Effect("Remove Victim", "subvic"),

                    new Effect("Invincible (30 seconds)", "invin"),
                    new Effect("Invincible P2 (30 seconds)", "invin2"),

                    new Effect("OHKO (30 seconds)", "ohko"),
                    new Effect("OHKO P2 (30 seconds)", "ohko2"),

                    new Effect("Infinite Ammo (30 seconds)", "unammo"),


                    new Effect("Invert Dpad (30 seconds)", "dpad"),
                    new Effect("Invert Dpad P2 (30 seconds)", "dpad2"),

                    new Effect("Pacifist (30 seconds)", "pacify"),
                    new Effect("Pacifist P2 (30 seconds)", "pacify2"),

                    new Effect("Monster Mode (15 seconds)", "monster"),
                    new Effect("Monster Mode P2 (15 seconds)", "monster2"),

                    new Effect("Ghost Mode (15 seconds)", "ghost"),
                    new Effect("Ghost Mode P2 (15 seconds)", "ghost2"),

                    new Effect("Hyde Mode (15 seconds)", "hyde"),
                    new Effect("Hyde Mode P2 (15 seconds)", "hyde2"),

                    new Effect("Disable Map (60 seconds)", "nomap"),

                    new Effect("Teleport P1 to P2", "tele"),
                    new Effect("Teleport P2 to P1", "tele2"),
                    new Effect("Swap P1 and P2 Position", "tele3"),

                };

                effects.AddRange(_weapons.Select(t => new Effect($"{t.Value.text}", $"addammo_{t.Key}", "addammo")));
                effects.AddRange(_weapons.Select(t => new Effect($"{t.Value.text}", $"takeammo_{t.Key}", "takeammo")));
                effects.AddRange(_weapons.Select(t => new Effect($"{t.Value.text}", $"addammo2_{t.Key}", "addammo2")));
                effects.AddRange(_weapons.Select(t => new Effect($"{t.Value.text}", $"takeammo2_{t.Key}", "takeammo2")));

                effects.AddRange(_items.Select(t => new Effect($"{t.Value.text}", $"additem_{t.Key}", "additem")));
                effects.AddRange(_items.Select(t => new Effect($"{t.Value.text}", $"takeitem_{t.Key}", "takeitem")));
                effects.AddRange(_items.Select(t => new Effect($"{t.Value.text}", $"additem2_{t.Key}", "additem2")));
                effects.AddRange(_items.Select(t => new Effect($"{t.Value.text}", $"takeitem2_{t.Key}", "takeitem2")));

                return effects;
            }
        }

        public override List<ItemType> ItemTypes => new List<ItemType>(new[]
        {
            new ItemType("Quantity", "quantity99", ItemType.Subtype.Slider, "{\"min\":1,\"max\":99}"),

        });

        public override List<ROMInfo> ROMTable => new List<ROMInfo>(new[]
        {
           new ROMInfo("Zombies Ate My Neighbors (U)", "ZombiesAteMyNeighbors.bps", (s, p) => Patching.BPS(s, p, false), ROMStatus.ValidUnpatched,s => Patching.MD5(s, "23c2af7897d9384c4791189b68c142eb")),
           new ROMInfo("Zombies Ate My Neighbors (Crowd Control)", null, Patching.Ignore, ROMStatus.ValidPatched,s => Patching.MD5(s, "e39b6d23ee467b90a0984fddfa5f78ac")),

        });

        public override Game Game { get; } = new Game(100, "Zombies Ate My Neighbors", "ZombiesAteMyNeighbors", "SNES", ConnectorType.SNESConnector);

        protected override bool IsReady(EffectRequest request) => Connector.Read8(ADDR_GAME_STATE, out byte b) && (b == 2) && Connector.Read8(ADDR_LOCKED, out byte l) && ((l & 0x80)==0) && Connector.Read8(ADDR_PAUSED, out byte p) && ((p & 0x80) != 0);


        protected override void RequestData(DataRequest request) => Respond(request, request.Key, null, false, $"Variable name \"{request.Key}\" not known");

        protected override void StartEffect(EffectRequest request)
        {
            if (!IsReady(request))
            {
                DelayEffect(request, TimeSpan.FromSeconds(5));
                return;
            }

            sbyte sign = 1;
            string[] codeParams = request.FinalCode.Split('_');
            switch (codeParams[0])
            {
                case "squash":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES, out ushort l) && (l != 0xFFFF) && Connector.Read16(ADDR_PLAYER_FUNC, out ushort f) && (f == 0xD53D),
                        () => Connector.Write16(ADDR_PLAYER_FUNC, 0xF9D0),
                        () =>
                        {
                            Connector.SendMessage($"{request.DisplayViewer} squashed you.");
                        }, true, "invin");
                    return;
                case "squash2":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE2, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l) && (l != 0xFFFF) && Connector.Read16(ADDR_PLAYER_FUNC2, out ushort f) && (f == 0xD53D),
                        () => Connector.Write16(ADDR_PLAYER_FUNC2, 0xF9D0),
                        () =>
                        {
                            Connector.SendMessage($"{request.DisplayViewer} squashed you.");
                        }, true, "invin2");
                    return;
                case "addvic":
                    TryEffect(request,
                        () => true,
                        () =>
                        {

                            if (!Connector.Read8(ADDR_NPCS_LEFT, out byte n)) return false;
                            if (!Connector.Read8(ADDR_NPCS_SAVED, out byte n2)) return false;

                            ConnectorLib.Log.Debug($"{n} - {n2}");

                            if (n + n2 >= 10) return false;

                            if(!Connector.Read32LE(ADDR_SCORE, out uint s))return false;


                            ConnectorLib.Log.Debug($"{s}");

                            uint ds = 0;
                            uint os = 0;

                            ds += 10000000 * ((s & 0xF0000000) / 0x10000000);
                            ds += 1000000 * ((s & 0xF000000) / 0x1000000);
                            ds += 100000 * ((s & 0xF00000) / 0x100000);
                            ds += 10000 * ((s & 0xF0000) / 0x10000);
                            ds += 1000 * ((s & 0xF000) / 0x1000);
                            ds += 100 * ((s & 0xF00) / 0x100);
                            ds += 10 * ((s & 0xF0) / 0x10);
                            ds += (s & 0xF);

                            ConnectorLib.Log.Debug($"{ds}");

                            byte b;

                            if (!Connector.Read8(ADDR_HISCORE, out b)) return false;
                            if (b != 0x2F) os += (uint)(10000000 * (b - 0x30));

                            if (!Connector.Read8(ADDR_HISCORE + 1, out b)) return false;
                            if (b != 0x2F) os += (uint)(1000000 * (b - 0x30));

                            if (!Connector.Read8(ADDR_HISCORE + 2, out b)) return false;
                            if (b != 0x2F) os += (uint)(100000 * (b - 0x30));

                            if (!Connector.Read8(ADDR_HISCORE + 3, out b)) return false;
                            if (b != 0x2F) os += (uint)(10000 * (b - 0x30));

                            if (!Connector.Read8(ADDR_HISCORE + 4, out b)) return false;
                            if (b != 0x2F) os += (uint)(1000 * (b - 0x30));

                            if (!Connector.Read8(ADDR_HISCORE + 5, out b)) return false;
                            if (b != 0x2F) os += (uint)(100 * (b - 0x30));

                            if (!Connector.Read8(ADDR_HISCORE + 6, out b)) return false;
                            if (b != 0x2F) os += (uint)(10 * (b - 0x30));

                            if (!Connector.Read8(ADDR_HISCORE + 7, out b)) return false;
                            if (b != 0x2F) os += (uint)(1 * (b - 0x30));

                            ConnectorLib.Log.Debug($"{os}");

                            if ((ds - os) >= 40000)
                            {
                                return Connector.Write8(ADDR_NPCS_SAVED, (byte)(n2+1));
                            }

                            s += 0x40000;

                            if ((s & 0xF0000) >= 0xA0000)
                            {
                                s -=  0xA0000;
                                s += 0x100000;

                                if ((s & 0xF00000) >= 0xA00000)
                                {
                                    s -=  0xA00000;
                                    s += 0x1000000;
                                }

                            }

                            if (!Connector.Write32LE(ADDR_SCORE, s)) return false;
                            return true;
                        },
                        () =>
                        {
                            PlaySFX(SFXType.PowerUp);
                            Connector.SendMessage($"{request.DisplayViewer} gave you a bonus victim.");
                        });
                    return;
                case "subvic":
                    TryEffect(request,
                        () => true,
                        () =>
                        {

                            if (!Connector.Read8(ADDR_NPCS_SAVED, out byte n2)) return false;

                            if (n2 < 2) return false;

                            return Connector.Write8(ADDR_NPCS_SAVED, (byte)(n2 - 1));
  
                        },
                        () =>
                        {
                            PlaySFX(SFXType.PowerUp);
                            Connector.SendMessage($"{request.DisplayViewer} removed a saved victim.");
                        });
                    return;
                case "nomap":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(30),
                            () => Connector.Read8(ADDR_SHOW_MAP, out byte m) && Connector.Read8(ADDR_SHOW_MAP2, out byte m2) && (m != 0 || m2 != 0),
                            () =>
                            {
                                bool result = Connector.Freeze16(ADDR_SHOW_MAP, 0x0000);
                                if(result)Connector.Freeze16(ADDR_SHOW_MAP2, 0x0000);
                                if (result) PlaySFX(SFXType.Buzz);
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "nomap");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} disabled the map."));
                        return;
                    }  
                    
                case "monster":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(30),
                            () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH, out byte h) && (h > 0) && Connector.Read16(ADDR_MONSTER_TIMER, out ushort t) && t == 0 && Connector.Read16(ADDR_MONSTER, out ushort m) && m == 0 && Connector.Read16(ADDR_MONSTER + 2, out ushort m2) && m2 == 0,
                            () =>
                            {
                                bool result = Connector.Freeze16(ADDR_MONSTER, 0x0001);
                                if (result) result = Connector.Freeze16(ADDR_MONSTER + 2, 0x0001);
                                if (result) PlaySFX(SFXType.Laugh);
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "invin");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} engaged monster mode!"));

                        return;
                    }
                case "monster2":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(30),
                            () => Connector.Read8(ADDR_ACTIVE2, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && (h > 0) && Connector.Read16(ADDR_MONSTER_TIMER2, out ushort t) && t == 0 && Connector.Read16(ADDR_MONSTER2, out ushort m) && m == 0 && Connector.Read16(ADDR_MONSTER2 + 2, out ushort m2) && m2 == 0,
                            () =>
                            {
                                bool result = Connector.Freeze16(ADDR_MONSTER2, 0x0001);
                                if (result) result = Connector.Freeze16(ADDR_MONSTER2 + 2, 0x0001);
                                if (result) { PlaySFX(SFXType.Laugh); Connector.SendMessage($"{request.DisplayViewer} engaged monster mode!"); }
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "invin2");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} engaged monster mode!"));

                        return;
                    }

                case "ghost":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(15),
                            () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_STATE, out byte st) && (st == 0) && Connector.Read8(ADDR_HEALTH, out byte h) && (h > 0) && Connector.Read16(ADDR_EQUIPPED_ITEM, out ushort p) && (p < 0x0B),
                            () =>
                            {
                                bool result = Connector.Freeze16(ADDR_EQUIPPED_ITEM, 0x000F);
                                byte b=0;
                                if (result) result = Connector.Freeze16(ADDR_EQUIPPED_WEAP, 0x0011);
                                if (result) result = Connector.Freeze8(ADDR_STATE, 4);

                                if (result) result = Connector.Read8(0x1AB6, out b);

                                b = (byte)(b | 0x10);

                                if (result) result = Connector.Write8(0x1AB6, b);
                                if (result) result = Connector.Write16(0x1AC6, 0x0C00);

                                if (result) PlaySFX(SFXType.Buzz);
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "ghost");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} made you a ghost!"));

                        return;
                    }
                case "ghost2":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(15),
                            () => Connector.Read8(ADDR_ACTIVE2, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_STATE2, out byte st) && (st == 0) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && (h > 0) && Connector.Read16(ADDR_EQUIPPED_ITEM2, out ushort p) && (p < 0x0B),
                            () =>
                            {
                                bool result = Connector.Freeze16(ADDR_EQUIPPED_ITEM2, 0x000F);
                                byte b = 0;
                                if (result) result = Connector.Freeze16(ADDR_EQUIPPED_WEAP2, 0x0011);
                                if (result) result = Connector.Freeze8(ADDR_STATE2, 4);

                                if (result) result = Connector.Read8(0x1A8E, out b);

                                b = (byte)(b | 0x10);

                                if (result) result = Connector.Write8(0x1A8E, b);
                                if (result) result = Connector.Write16(0x1A9E, 0x0C00);

                                if (result) PlaySFX(SFXType.Buzz);
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "ghost2");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} made you a ghost!"));

                        return;
                    }

                case "hyde":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(15),
                            () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_STATE, out byte st) && (st == 0) && Connector.Read8(ADDR_HEALTH, out byte h) && (h > 0) && Connector.Read16(ADDR_EQUIPPED_ITEM, out ushort p) && (p < 0x0B),
                            () =>
                            {
                                bool result = Connector.Freeze16(ADDR_EQUIPPED_ITEM, 0x000E);
                                byte b = 0;
                                if (result) result = Connector.Freeze16(ADDR_EQUIPPED_WEAP, 0x0010);
                                if (result) result = Connector.Freeze8(ADDR_STATE, 8);

                                if (result) result = Connector.Read8(0x1AB6, out b);

                                b = (byte)(b | 0x10);

                                if (result) result = Connector.Write8(0x1AB6, b);
                                if (result) result = Connector.Write16(0x1AC6, 0x0600);
                                if (result) result = Connector.Write8(0x1AC4, 0x03);

                                if (result) result = Connector.Write8(0x176, 0x04);


                                if (result) PlaySFX(SFXType.Buzz);
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "ghost");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} turned you into Mr. Hyde!"));

                        return;
                    }
                 case "hyde2":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(15),
                            () => Connector.Read8(ADDR_ACTIVE2, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_STATE2, out byte st) && (st == 0) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && (h > 0) && Connector.Read16(ADDR_EQUIPPED_ITEM2, out ushort p) && (p < 0x0B),
                            () =>
                            {
                                bool result = Connector.Freeze16(ADDR_EQUIPPED_ITEM2, 0x000E);
                                byte b = 0;
                                if (result) result = Connector.Freeze16(ADDR_EQUIPPED_WEAP2, 0x0010);
                                if (result) result = Connector.Freeze8(ADDR_STATE2, 8);

                                if (result) result = Connector.Read8(0x1A8E, out b);

                                b = (byte)(b | 0x10);

                                if (result) result = Connector.Write8(0x1A8E, b);
                                if (result) result = Connector.Write16(0x1A9E, 0x0600);
                                if (result) result = Connector.Write8(0x1A9C, 0x03);

                                if (result) result = Connector.Write8(0x2F6, 0x04);


                                if (result) PlaySFX(SFXType.Buzz);
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "ghost2");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} turned you into Mr. Hyde!"));

                        return;
                    }
                case "pacify":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(30),
                            () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH, out byte h) && (h > 0) && Connector.Read16(ADDR_PACIFY, out ushort p) && p == 0,
                            () =>
                            {
                                bool result = Connector.Freeze16(ADDR_PACIFY, 0x0100);
                                if (result) PlaySFX(SFXType.Laugh);
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "pacify");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} made you unable to attack."));

                        return;
                    }
                case "pacify2":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(30),
                        () => Connector.Read8(ADDR_ACTIVE2, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && (h > 0) && Connector.Read16(ADDR_PACIFY2, out ushort p) && p == 0,
                        () =>
                        {
                            bool result = Connector.Freeze16(ADDR_PACIFY2, 0x0100);
                            if (result) PlaySFX(SFXType.Laugh);
                            return result;
                        },
                        TimeSpan.FromSeconds(1),
                        () => IsReady(request), TimeSpan.FromSeconds(1),
                        () => true, TimeSpan.FromSeconds(1), true
                        , "pacify2");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} made you unable to attack."));

                        return;
                    }
                case "dpad":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(30),
                            () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH, out byte h) && (h > 0),
                            () =>
                            {
                                bool result = Connector.Write16(ADDR_DPAD_TRIGGER, 0xABAB);
                                if (result) PlaySFX(SFXType.Laugh);
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "dpad");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} inverted your dpad."));

                        return;
                    }
                case "dpad2":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(30),
                            () => Connector.Read8(ADDR_ACTIVE2, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && (h > 0),
                            () =>
                            {
                                bool result = Connector.Write16(ADDR_DPAD2_TRIGGER, 0xABAB);
                                if (result) PlaySFX(SFXType.Laugh);
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "dpad2");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} inverted your dpad."));

                        return;
                    }
                case "unammo":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(30),
                            () => true,
                            () =>
                            {
                                bool result = Connector.Write16(ADDR_AMMO_TRIGGER, 0xABAB);
                                if (result) PlaySFX(SFXType.PowerUp);
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "unammo");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} gave you unlimited ammo."));

                        return;
                    }
                case "ohko":
                    {
                        var s = RepeatAction(request, TimeSpan.FromSeconds(30),
                            () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read16(ADDR_LIVES, out ushort l) && (l != 0xFFFF) && Connector.Read8(ADDR_HEALTH, out byte h) && h != 0,
                            () => {
                                bool s = Connector.Read8(ADDR_HEALTH, out oldhealth) && Connector.Write8(ADDR_HEALTH, 1);

                                if (s) PlaySFX(SFXType.Laugh);
                                return s;
                                },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => {
                                if(!Connector.Read8(ADDR_HEALTH, out byte h))return false;
                                if (h > 0)
                                    if (!Connector.Write8(ADDR_HEALTH, 1)) return false;
                                return true;
                                }, TimeSpan.FromSeconds(1), true
                            , "invin");
                        s.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} triggered OHKO."));
                        s.WhenCompleted.Then(t => { Connector.Write8(ADDR_HEALTH, oldhealth);  Connector.SendMessage($"{request.DisplayViewer}'s OHKO has ended."); });
                        return;
                    }
                case "ohko2":
                    {
                        var s = RepeatAction(request, TimeSpan.FromSeconds(30),
                            () => Connector.Read8(ADDR_ACTIVE2, out byte ac) && (ac == 0x80) && Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l) && (l != 0xFFFF) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && h != 0,
                            () => {
                                bool s = Connector.Read8(ADDR_HEALTH + (ushort)2, out oldhealth2) && Connector.Write8(ADDR_HEALTH + (ushort)2, 1);

                                if (s) PlaySFX(SFXType.Laugh);
                                return s;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => {
                                if (!Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h)) return false;
                                if (h > 0)
                                    if (!Connector.Write8(ADDR_HEALTH + (ushort)2, 1)) return false;
                                return true;
                            }, TimeSpan.FromSeconds(1), true
                            , "invin2");
                        s.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} triggered OHKO."));
                        s.WhenCompleted.Then(t => { Connector.Write8(ADDR_HEALTH + (ushort)2, oldhealth2); Connector.SendMessage($"{request.DisplayViewer}'s OHKO has ended."); });
                        return;
                    }
                case "invin":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(30),
                            () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && (Connector.Read16(ADDR_LIVES, out ushort l) && (l != 0xFFFF)) && Connector.Read8(ADDR_HEALTH, out byte h) && h != 0,
                            () =>
                            {
                                if (!Connector.Read8(ADDR_HEALTH, out byte h)) return false;

                                bool result = Connector.Freeze8(ADDR_HEALTH, h);
                                if (result) PlaySFX(SFXType.Twinkle); 
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "invin");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} made you invincible."));

                        return;
                    }
                case "invin2":
                    {
                        var e = RepeatAction(request, TimeSpan.FromSeconds(30),
                            () => Connector.Read8(ADDR_ACTIVE2, out byte ac) && (ac == 0x80) && (Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l) && (l != 0xFFFF)) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && h != 0,
                            () =>
                            {
                                if (!Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h)) return false;

                                bool result = Connector.Freeze8(ADDR_HEALTH + (ushort)2, h);
                                if (result) PlaySFX(SFXType.Twinkle);
                                return result;
                            },
                            TimeSpan.FromSeconds(1),
                            () => IsReady(request), TimeSpan.FromSeconds(1),
                            () => true, TimeSpan.FromSeconds(1), true
                            , "invin2");
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} made you invincible."));

                        return;
                    }
                case "swap":
                    TryEffect(request,
                        () => true,
                        () =>
                        {
                            if (!Connector.Read8(ADDR_PLAYER1, out byte p)) return false;

                            if (!Connector.Write8(ADDR_PLAYER1, (byte)(2 - p))) return false;
                            if (!Connector.Write8(ADDR_PLAYER2, p)) return false;
                            if (!Connector.Write8(ADDR_PLAYER1B, (byte)(2 - p))) return false;
                            if (!Connector.Write8(ADDR_PLAYER2B, p)) return false;

                             PlaySFX(SFXType.Pickup);
                            return true;
                        },
                        () =>
                        {
                            Connector.SendMessage($"{request.DisplayViewer} swapped characters.");
                        });
                    return;
                case "door":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_NPCS_LEFT, out byte l) && (l != 0),
                        () => {
                            if (!Connector.Read8(ADDR_NPCS_SAVED, out byte s)) return false;

                            if (s == 0)
                            {
                                if (!Connector.Write8(ADDR_NPCS_SAVED, 1)) return false;
                            }

                            if (!Connector.Write8(ADDR_NPCS_LEFT, 0)) return false;

                            return true;
                            },
                        () =>
                        {
                            PlaySFX(SFXType.PowerUp);
                            Connector.SendMessage($"{request.DisplayViewer} triggered the door.");
                        });
                    return;
                case "oneup":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_HEALTH, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES, out ushort l) && (l != 0xFFFF),
                        () => Connector.RangeAdd8(ADDR_LIVES, 1, 0, 6, false),
                        () =>
                        {
                            PlaySFX(SFXType.PowerUp);
                            Connector.SendMessage($"{request.DisplayViewer} gave you an extra life.");
                        });
                    return;
                case "onedown":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_HEALTH, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES, out ushort l) && (l != 0xFFFF),
                        () => Connector.RangeAdd8(ADDR_LIVES, -1, 0, 6, false),
                        () =>
                        {
                            PlaySFX(SFXType.Death);
                            Connector.SendMessage($"{request.DisplayViewer} took an extra life.");
                        });
                    return;
                case "oneup2":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l) && (l != 0xFFFF),
                        () => Connector.RangeAdd8(ADDR_LIVES + (ushort)2, 1, 0, 6, false),
                        () =>
                        {
                            PlaySFX(SFXType.PowerUp);
                            Connector.SendMessage($"{request.DisplayViewer} gave you an extra life.");
                        });
                    return;
                case "onedown2":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l) && (l != 0xFFFF),
                        () => Connector.RangeAdd8(ADDR_LIVES + (ushort)2, -1, 0, 6, false),
                        () =>
                        {
                            PlaySFX(SFXType.Death);
                            Connector.SendMessage($"{request.DisplayViewer} took an extra life.");
                        });
                    return;
                case "tele":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_ACTIVE2, out byte ac2) && (ac2 == 0x80) && Connector.Read8(ADDR_HEALTH, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES, out ushort l) && (l != 0xFFFF) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h2) && (h2 != 0) && Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l2) && (l2 != 0xFFFF),
                        () =>
                        {
                            if (!Connector.Read16(ADDR_XPOS2, out ushort x)) return false;
                            if (!Connector.Read16(ADDR_XPOSB2, out ushort xb)) return false;

                            if (x != xb) return false;

                            if (!Connector.Read16(ADDR_YPOS2, out ushort y)) return false;
                            if (!Connector.Read16(ADDR_YPOSB2, out ushort yb)) return false;

                            if (y != yb) return false;

                            if (!Connector.Write16(ADDR_XPOS, x)) return false;
                            if (!Connector.Write16(ADDR_XPOSB, xb)) return false;
                            if (!Connector.Write16(ADDR_YPOS, y)) return false;
                            if (!Connector.Write16(ADDR_YPOSB, yb)) return false;

                            return true;
                        },
                        () =>
                        {
                            PlaySFX(SFXType.Rescue);
                            Connector.SendMessage($"{request.DisplayViewer} changed your position.");
                        });
                    return;
                case "tele2":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_ACTIVE2, out byte ac2) && (ac2 == 0x80) && Connector.Read8(ADDR_HEALTH, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES, out ushort l) && (l != 0xFFFF) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h2) && (h2 != 0) && Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l2) && (l2 != 0xFFFF),
                        () =>
                        {
                            if (!Connector.Read16(ADDR_XPOS, out ushort x)) return false;
                            if (!Connector.Read16(ADDR_XPOSB, out ushort xb)) return false;

                            if (x != xb) return false;

                            if (!Connector.Read16(ADDR_YPOS, out ushort y)) return false;
                            if (!Connector.Read16(ADDR_YPOSB, out ushort yb)) return false;

                            if (y != yb) return false;

                            if (!Connector.Write16(ADDR_XPOS2, x)) return false;
                            if (!Connector.Write16(ADDR_XPOSB2, xb)) return false;
                            if (!Connector.Write16(ADDR_YPOS2, y)) return false;
                            if (!Connector.Write16(ADDR_YPOSB2, yb)) return false;

                            return true;
                        },
                        () =>
                        {
                            PlaySFX(SFXType.Rescue);
                            Connector.SendMessage($"{request.DisplayViewer} changed your position.");
                        });
                    return;
                case "tele3":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_ACTIVE2, out byte ac2) && (ac2 == 0x80) && Connector.Read8(ADDR_HEALTH, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES, out ushort l) && (l != 0xFFFF) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h2) && (h2 != 0) && Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l2) && (l2 != 0xFFFF),
                        () =>
                        {
                            if (!Connector.Read16(ADDR_XPOS, out ushort x)) return false;
                            if (!Connector.Read16(ADDR_XPOSB, out ushort xb)) return false;

                            if (x != xb) return false;

                            if (!Connector.Read16(ADDR_YPOS, out ushort y)) return false;
                            if (!Connector.Read16(ADDR_YPOSB, out ushort yb)) return false;

                            if (y != yb) return false;


                            if (!Connector.Read16(ADDR_XPOS2, out ushort x2)) return false;
                            if (!Connector.Read16(ADDR_XPOSB2, out ushort xb2)) return false;

                            if (x2 != xb2) return false;

                            if (!Connector.Read16(ADDR_YPOS2, out ushort y2)) return false;
                            if (!Connector.Read16(ADDR_YPOSB2, out ushort yb2)) return false;

                            if (y2 != yb2) return false;


                            if (!Connector.Write16(ADDR_XPOS2, x)) return false;
                            if (!Connector.Write16(ADDR_XPOSB2, xb)) return false;
                            if (!Connector.Write16(ADDR_YPOS2, y)) return false;
                            if (!Connector.Write16(ADDR_YPOSB2, yb)) return false;

                            if (!Connector.Write16(ADDR_XPOS, x2)) return false;
                            if (!Connector.Write16(ADDR_XPOSB, xb2)) return false;
                            if (!Connector.Write16(ADDR_YPOS, y2)) return false;
                            if (!Connector.Write16(ADDR_YPOSB, yb2)) return false;

                            return true;
                        },
                        () =>
                        {
                            PlaySFX(SFXType.Rescue);
                            Connector.SendMessage($"{request.DisplayViewer} changed your position.");
                        });
                    return;
                case "heal":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES, out ushort l) && (l!=0xFFFF),
                        () => Connector.RangeAdd8(ADDR_HEALTH, 1, 0, 10, false),
                        () =>
                        {
                            PlaySFX(SFXType.PowerUp);
                            Connector.SendMessage($"{request.DisplayViewer} healed you.");
                        },true,"invin");
                    return;
                case "hurt":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES, out ushort l) && (l != 0xFFFF),
                        () => Connector.RangeAdd8(ADDR_HEALTH, -1, 0, 10, false),
                        () =>
                        {
                            PlaySFX(SFXType.Hurt);
                            Connector.SendMessage($"{request.DisplayViewer} hurt you.");
                        }, () => TimeSpan.FromSeconds(5), true, new string[] { "invin", "ghost" });
                    return;
                case "fullheal":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH, out byte h) && (h!=10) && (h != 0) && Connector.Read16(ADDR_LIVES, out ushort l) && (l != 0xFFFF),
                        () => Connector.Write8(ADDR_HEALTH, 10),
                        () =>
                        {
                            PlaySFX(SFXType.PowerUp);
                            Connector.SendMessage($"{request.DisplayViewer} fully healed you.");
                        }, true, "invin");
                    return;
                case "kill":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES, out ushort l) && (l != 0xFFFF),
                        () => Connector.Write8(ADDR_HEALTH, 0),
                        () =>
                        {
                            Connector.SendMessage($"{request.DisplayViewer} killed you.");
                        }, () => TimeSpan.FromSeconds(5), true, new string[] { "invin", "ghost" });
                    return;
                case "heal2":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE2, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l) && (l != 0xFFFF),
                        () => Connector.RangeAdd8(ADDR_HEALTH + (ushort)2, 1, 0, 10, false),
                        () =>
                        {
                            PlaySFX(SFXType.PowerUp);
                            Connector.SendMessage($"{request.DisplayViewer} healed you.");
                        }, true, "invin2");
                    return;
                case "hurt2":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE2, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l) && (l != 0xFFFF),
                        () => Connector.RangeAdd8(ADDR_HEALTH + (ushort)2, -1, 0, 10, false),
                        () =>
                        {
                            PlaySFX(SFXType.Hurt);
                            Connector.SendMessage($"{request.DisplayViewer} hurt you.");
                        }, () => TimeSpan.FromSeconds(5),true, new string[] { "invin2", "ghost2" });
                    return;
                case "fullheal2":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE2, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && (h != 10) && (h != 0) && Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l) && (l != 0xFFFF),
                        () => Connector.Write8(ADDR_HEALTH + (ushort)2, 10),
                        () =>
                        {
                            PlaySFX(SFXType.PowerUp);
                            Connector.SendMessage($"{request.DisplayViewer} fully healed you.");
                        }, true, "invin2");
                    return;
                case "kill2":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_ACTIVE2, out byte ac) && (ac == 0x80) && Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h) && (h != 0) && Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l) && (l != 0xFFFF),
                        () => Connector.Write8(ADDR_HEALTH + (ushort)2, 0),
                        () =>
                        {
                            Connector.SendMessage($"{request.DisplayViewer} killed you.");
                        }, () => TimeSpan.FromSeconds(5), true, new string[] { "invin2", "ghost2" });
                    return;
                case "addammo":
                    {

                        var item = _weapons[codeParams[1]];


                        TryEffect(request,
                            () => {
                                if (!Connector.Read16(ADDR_LIVES, out ushort l)) return false;
                                if (l == 0xFFFF) return false;

                                if (!Connector.Read8(ADDR_HEALTH, out byte h)) return false;
                                if (h == 0) return false;

                                if (!Connector.Read8(ADDR_ACTIVE, out byte ac)) return false;
                                if (ac != 0x80) return false;

                                if (!Connector.Read16(ADDR_AMMO_SQUIRT + (ulong)(2 * item.id), out ushort ammo))return false;

                                ammo = BCDToDec(ammo);

                                if (ammo >= (ushort)item.max) return false;

                                ammo += (ushort)item.amount;
                                if (ammo >= item.max) ammo = (ushort)item.max;

                                ammo = DecToBCD(ammo);

                                if (!Connector.Write16(ADDR_AMMO_SQUIRT + (ulong)(2 * item.id), ammo)) return false;

                                return true;
                            },
                            () => {
                                PlaySFX(SFXType.PowerUp);
                                Connector.SendMessage($"{request.DisplayViewer} gave you {item.text} ammo.");

                                return true;
                            });

                        return;
                    }
                case "takeammo":
                    {

                        var item = _weapons[codeParams[1]];


                        TryEffect(request,
                            () => {
                                if (!Connector.Read16(ADDR_AMMO_SQUIRT + (ulong)(2 * item.id), out ushort ammo)) return false;
                                if (!Connector.Read16(ADDR_LIVES, out ushort l)) return false;
                                if (l == 0xFFFF) return false;

                                if (!Connector.Read8(ADDR_ACTIVE, out byte ac)) return false;
                                if (ac != 0x80) return false;

                                if (!Connector.Read8(ADDR_HEALTH, out byte h)) return false;
                                if (h == 0) return false;

                                ammo = BCDToDec(ammo);

                                if (ammo == 0) return false;

                                if (ammo <= item.amount) ammo = 0;
                                else ammo -= (ushort)item.amount;
                                
                                ammo = DecToBCD(ammo);

                                if (!Connector.Write16(ADDR_AMMO_SQUIRT + (ulong)(2 * item.id), ammo)) return false;

                                return true;
                            },
                            () => {
                                PlaySFX(SFXType.Buzz);
                                Connector.SendMessage($"{request.DisplayViewer} took your {item.text} ammo.");

                                return true;
                            });

                        return;
                    }

                case "addammo2":
                    {

                        var item = _weapons[codeParams[1]];


                        TryEffect(request,
                            () => {
                                if (!Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l)) return false;
                                if (l == 0xFFFF) return false;

                                if (!Connector.Read8(ADDR_ACTIVE2, out byte ac)) return false;
                                if (ac != 0x80) return false;

                                if (!Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h)) return false;
                                if (h == 0) return false;

                                if (!Connector.Read16(ADDR_AMMO_SQUIRT + (ulong)(32 + 2 * item.id), out ushort ammo)) return false;

                                ammo = BCDToDec(ammo);

                                if (ammo >= (ushort)item.max) return false;

                                ammo += (ushort)item.amount;
                                if (ammo >= item.max) ammo = (ushort)item.max;

                                ammo = DecToBCD(ammo);

                                if (!Connector.Write16(ADDR_AMMO_SQUIRT + (ulong)(32 + 2 * item.id), ammo)) return false;

                                return true;
                            },
                            () => {
                                PlaySFX(SFXType.PowerUp);
                                Connector.SendMessage($"{request.DisplayViewer} gave you {item.text} ammo.");

                                return true;
                            });

                        return;
                    }
                case "takeammo2":
                    {

                        var item = _weapons[codeParams[1]];


                        TryEffect(request,
                            () => {
                                if (!Connector.Read16(ADDR_AMMO_SQUIRT + (ulong)(32 + 2 * item.id), out ushort ammo)) return false;
                                if (!Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l)) return false;
                                if (l == 0xFFFF) return false;

                                if (!Connector.Read8(ADDR_ACTIVE2, out byte ac)) return false;
                                if (ac != 0x80) return false;

                                if (!Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h)) return false;
                                if (h == 0) return false;

                                ammo = BCDToDec(ammo);

                                if (ammo == 0) return false;

                                if (ammo <= item.amount) ammo = 0;
                                else ammo -= (ushort)item.amount;

                                ammo = DecToBCD(ammo);

                                if (!Connector.Write16(ADDR_AMMO_SQUIRT + (ulong)(32 + 2 * item.id), ammo)) return false;

                                return true;
                            },
                            () => {
                                PlaySFX(SFXType.Buzz);
                                Connector.SendMessage($"{request.DisplayViewer} took your {item.text} ammo.");

                                return true;
                            });

                        return;
                    }

                case "additem":
                    {

                        var item = _items[codeParams[1]];


                        TryEffect(request,
                            () => {
                                if (!Connector.Read16(ADDR_LIVES, out ushort l)) return false;
                                if (l == 0xFFFF) return false;

                                if (!Connector.Read8(ADDR_ACTIVE, out byte ac)) return false;
                                if (ac != 0x80) return false;

                                if (!Connector.Read8(ADDR_HEALTH, out byte h)) return false;
                                if (h == 0) return false;

                                if (!Connector.Read16(ADDR_ITEM_KEYS + (ulong)(2 * item.id), out ushort ammo)) return false;

                                ammo = BCDToDec(ammo);

                                if (ammo >= (ushort)999) return false;

                                ammo += (ushort)1;

                                ammo = DecToBCD(ammo);

                                if (!Connector.Write16(ADDR_ITEM_KEYS + (ulong)(2 * item.id), ammo)) return false;

                                return true;
                            },
                            () => {
                                PlaySFX(SFXType.Pickup);
                                Connector.SendMessage($"{request.DisplayViewer} gave you a {item.text}.");

                                return true;
                            });

                        return;
                    }
                case "takeitem":
                    {

                        var item = _items[codeParams[1]];


                        TryEffect(request,
                            () => {
                                if (!Connector.Read16(ADDR_ITEM_KEYS + (ulong)(2 * item.id), out ushort ammo)) return false;
                                if (!Connector.Read16(ADDR_LIVES, out ushort l)) return false;
                                if (l == 0xFFFF) return false;

                                if (!Connector.Read8(ADDR_ACTIVE, out byte ac)) return false;
                                if (ac != 0x80) return false;

                                if (!Connector.Read8(ADDR_HEALTH, out byte h)) return false;
                                if (h == 0) return false;

                                ammo = BCDToDec(ammo);

                                if (ammo == 0) return false;

                                ammo -= (ushort)1;

                                ammo = DecToBCD(ammo);

                                if (!Connector.Write16(ADDR_ITEM_KEYS + (ulong)(2 * item.id), ammo)) return false;

                                return true;
                            },
                            () => {
                                PlaySFX(SFXType.Buzz);
                                Connector.SendMessage($"{request.DisplayViewer} took a {item.text}.");

                                return true;
                            });

                        return;
                    }

                case "additem2":
                    {

                        var item = _items[codeParams[1]];


                        TryEffect(request,
                            () => {
                                if (!Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l)) return false;
                                if (l == 0xFFFF) return false;

                                if (!Connector.Read8(ADDR_ACTIVE2, out byte ac)) return false;
                                if (ac != 0x80) return false;

                                if (!Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h)) return false;
                                if (h == 0) return false;

                                if (!Connector.Read16(ADDR_ITEM_KEYS + (ulong)(32 + 2 * item.id), out ushort ammo)) return false;

                                ammo = BCDToDec(ammo);

                                if (ammo >= (ushort)999) return false;

                                ammo += (ushort)1;

                                ammo = DecToBCD(ammo);

                                if (!Connector.Write16(ADDR_ITEM_KEYS + (ulong)(32 + 2 * item.id), ammo)) return false;

                                return true;
                            },
                            () => {
                                PlaySFX(SFXType.Pickup);
                                Connector.SendMessage($"{request.DisplayViewer} gave you a {item.text}.");

                                return true;
                            });

                        return;
                    }
                case "takeitem2":
                    {

                        var item = _items[codeParams[1]];


                        TryEffect(request,
                            () => {
                                if (!Connector.Read16(ADDR_ITEM_KEYS + (ulong)(32 + 2 * item.id), out ushort ammo)) return false;
                                if (!Connector.Read16(ADDR_LIVES + (ushort)2, out ushort l)) return false;
                                if (l == 0xFFFF) return false;

                                if (!Connector.Read8(ADDR_ACTIVE2, out byte ac)) return false;
                                if (ac != 0x80) return false;

                                if (!Connector.Read8(ADDR_HEALTH + (ushort)2, out byte h)) return false;
                                if (h == 0) return false;

                                ammo = BCDToDec(ammo);

                                if (ammo == 0) return false;

                                ammo -= (ushort)1;

                                ammo = DecToBCD(ammo);

                                if (!Connector.Write16(ADDR_ITEM_KEYS + (ulong)(32 + 2 * item.id), ammo)) return false;

                                return true;
                            },
                            () => {
                                PlaySFX(SFXType.Buzz);
                                Connector.SendMessage($"{request.DisplayViewer} took a {item.text}.");

                                return true;
                            });

                        return;
                    }

            }
        }
        
        private ushort BCDToDec(ushort val)
        {
            ushort thou = (ushort)((val & 0xF000) / 0x1000);
            ushort hund = (ushort)((val & 0xF00) / 0x100);
            ushort tens = (ushort)((val & 0xF0) / 0x10);
            ushort ones = (ushort)((val & 0xF));

            return (ushort)(thou * 1000 + hund * 100 + tens * 10 + ones);
        }

        private ushort DecToBCD(ushort val)
        {
            ushort ones = (ushort)(val % 10);
            val -= ones;
            val /= 10;

            ushort tens = (ushort)(val % 10);
            val -= tens;
            val /= 10;

            ushort hund = (ushort)(val % 10);
            val -= hund;
            val /= 10;

            ushort thou = (ushort)(val % 10);

            return (ushort)(ones + tens * 0x10 + hund * 0x100 + thou * 0x1000);
            
            //return (ushort)(thou * 1000 + hund * 100 + tens * 10 + ones);
        }

        private bool StopAll()
        {

            if (!Connector.Write16(ADDR_AMMO_TRIGGER, 0)) return false;
            if (!Connector.Unfreeze(ADDR_HEALTH)) return false;
            if (!Connector.Unfreeze(ADDR_HEALTH + (ushort)2)) return false;
            if (!Connector.Write16(ADDR_DPAD_TRIGGER, 0))return false;
            if (!Connector.Write16(ADDR_DPAD2_TRIGGER, 0)) return false;


            if (!Connector.Unfreeze(ADDR_SHOW_MAP))return false;
            if (!Connector.Unfreeze(ADDR_SHOW_MAP2)) return false;
            if (!Connector.Unfreeze(ADDR_MONSTER)) return false;
            if (!Connector.Unfreeze(ADDR_MONSTER + 2)) return false;
            if (!Connector.Unfreeze(ADDR_MONSTER2)) return false;
            if (!Connector.Unfreeze(ADDR_MONSTER2 + 2)) return false;

            if (!Connector.Unfreeze(ADDR_EQUIPPED_ITEM)) return false;
            if (!Connector.Unfreeze(ADDR_EQUIPPED_WEAP)) return false;
            if (!Connector.Unfreeze(ADDR_EQUIPPED_ITEM2)) return false;
            if (!Connector.Unfreeze(ADDR_EQUIPPED_WEAP2)) return false;

            if (!Connector.Unfreeze(ADDR_STATE)) return false;
            if (!Connector.Unfreeze(ADDR_STATE2)) return false;

            if (!Connector.Unfreeze(ADDR_PACIFY)) return false;
            if (!Connector.Unfreeze(ADDR_PACIFY2)) return false;

            return true;
        }

        protected override bool StopEffect(EffectRequest request)
        {
            bool success;
            switch (request.BaseCode)
            {
                case "nomap":
                    success = Connector.Unfreeze(ADDR_SHOW_MAP);
                    success &= Connector.Unfreeze(ADDR_SHOW_MAP2);
                    if (success)
                    {
                        Connector.SendMessage($"{request.DisplayViewer}'s map disable has ended.");
                    }
                    return success;
                case "monster":
                    success = Connector.Unfreeze(ADDR_MONSTER);
                    success &= success = Connector.Write16(ADDR_MONSTER, 0);
                    success &= Connector.Unfreeze(ADDR_MONSTER+2);
                    success &= success = Connector.Write16(ADDR_MONSTER+2, 0);
                    if (success)
                    {
                        Connector.SendMessage($"{request.DisplayViewer}'s monster mode has ended.");
                    }
                    return success;
                case "monster2":
                    success = Connector.Unfreeze(ADDR_MONSTER2);
                    success &= success = Connector.Write16(ADDR_MONSTER2, 0);
                    success &= Connector.Unfreeze(ADDR_MONSTER2 + 2);
                    success &= success = Connector.Write16(ADDR_MONSTER2 + 2, 0);
                    if (success)
                    {
                        Connector.SendMessage($"{request.DisplayViewer}'s monster mode has ended.");
                    }
                    return success;
                case "ghost":
                    {
                        success = Connector.Unfreeze(ADDR_EQUIPPED_ITEM);
                        success &= success = Connector.Write16(ADDR_EQUIPPED_ITEM, 0);

                        success &= Connector.Unfreeze(ADDR_EQUIPPED_WEAP);
                        success &= success = Connector.Write16(ADDR_EQUIPPED_WEAP, 0);

                        success &= Connector.Unfreeze(ADDR_STATE);
                        success &= success = Connector.Write8(ADDR_STATE, 0);

                        success &= Connector.Read8(0x1AB6, out byte b);

                        b = (byte)(b & 0xEF);

                        success &= Connector.Write8(0x1AB6, b);


                        if (success)
                        {
                            Connector.SendMessage($"{request.DisplayViewer}'s ghost mode has ended.");
                        }
                        return success;
                    }
                case "ghost2":
                    {
                        success = Connector.Unfreeze(ADDR_EQUIPPED_ITEM2);
                        success &= success = Connector.Write16(ADDR_EQUIPPED_ITEM2, 0);

                        success &= Connector.Unfreeze(ADDR_EQUIPPED_WEAP2);
                        success &= success = Connector.Write16(ADDR_EQUIPPED_WEAP2, 0);

                        success &= Connector.Unfreeze(ADDR_STATE2);
                        success &= success = Connector.Write8(ADDR_STATE2, 0);


                        success &= Connector.Read8(0x1A8E, out byte b);

                        b = (byte)(b & 0xEF);

                        success &= Connector.Write8(0x1A8E, b);

                        if (success)
                        {
                            Connector.SendMessage($"{request.DisplayViewer}'s ghost mode has ended.");
                        }
                        return success;
                    }
                case "hyde":
                    {
                        success = Connector.Unfreeze(ADDR_EQUIPPED_ITEM);
                        success &= success = Connector.Write16(ADDR_EQUIPPED_ITEM, 0);

                        success &= Connector.Unfreeze(ADDR_EQUIPPED_WEAP);
                        success &= success = Connector.Write16(ADDR_EQUIPPED_WEAP, 0);

                        success &= Connector.Unfreeze(ADDR_STATE);
                        success &= success = Connector.Write8(ADDR_STATE, 0);

                        success &= Connector.Read8(0x1AB6, out byte b);

                        b = (byte)(b & 0xEF);

                        success &= Connector.Write8(0x1AB6, b);

                        success &= Connector.Write8(0x1AC4, 0x05);

                        success &= Connector.Write8(0x176, 0x00);

                        if (success)
                        {
                            Connector.SendMessage($"{request.DisplayViewer}'s Hyde mode has ended.");
                        }
                        return success;
                    }
                case "hyde2":
                    {
                        success = Connector.Unfreeze(ADDR_EQUIPPED_ITEM2);
                        success &= success = Connector.Write16(ADDR_EQUIPPED_ITEM2, 0);

                        success &= Connector.Unfreeze(ADDR_EQUIPPED_WEAP2);
                        success &= success = Connector.Write16(ADDR_EQUIPPED_WEAP2, 0);

                        success &= Connector.Unfreeze(ADDR_STATE2);
                        success &= success = Connector.Write8(ADDR_STATE2, 0);

                        success &= Connector.Read8(0x1A8E, out byte b);

                        b = (byte)(b & 0xEF);

                        success &= Connector.Write8(0x1A8E, b);

                        success &= Connector.Write8(0x1A9C, 0x05);

                        success &= Connector.Write8(0x2F6, 0x00);

                        if (success)
                        {
                            Connector.SendMessage($"{request.DisplayViewer}'s Hyde mode has ended.");
                        }
                        return success;
                    }
                case "pacify":
                    success = Connector.Unfreeze(ADDR_PACIFY);
                    success &= success = Connector.Write16(ADDR_PACIFY,0);
                    if (success)
                    {
                        Connector.SendMessage($"{request.DisplayViewer}'s pacifism mode has ended.");
                    }
                    return success;
                case "pacify2":
                    success = Connector.Unfreeze(ADDR_PACIFY2);
                    success &= success = Connector.Write16(ADDR_PACIFY2, 0);
                    if (success)
                    {
                        Connector.SendMessage($"{request.DisplayViewer}'s pacifism mode has ended.");
                    }
                    return success;
                case "dpad":
                    success = Connector.Write16(ADDR_DPAD_TRIGGER, 0);
                    if (success)
                    {
                        Connector.SendMessage($"{request.DisplayViewer}'s dpad inversion has ended.");
                    }
                    return success;
                case "dpad2":
                    success = Connector.Write16(ADDR_DPAD2_TRIGGER, 0);
                    if (success)
                    {
                        Connector.SendMessage($"{request.DisplayViewer}'s dpad inversion has ended.");
                    }
                    return success;
                case "unammo":
                    success = Connector.Write16(ADDR_AMMO_TRIGGER, 0);
                    if (success)
                    {
                        Connector.SendMessage($"{request.DisplayViewer}'s unlimited ammo has ended.");
                    }
                    return success;
                case "invin":
                    success = Connector.Unfreeze(ADDR_HEALTH);
                    if (success)
                    {
                        Connector.SendMessage($"{request.DisplayViewer}'s invulnerability has ended.");
                    }
                    return success;
                case "invin2":
                    success = Connector.Unfreeze(ADDR_HEALTH + (ushort)2);
                    if (success)
                    {
                        Connector.SendMessage($"{request.DisplayViewer}'s invulnerability has ended.");
                    }
                    return success;
                default:
                    return true;
            }
        }

        public override bool StopAllEffects() => StopAll();


        private void PlaySFX(SFXType type)
        {
            ushort value = (ushort)type;
            Connector.Write16(ADDR_SFX_ID, value);
            Connector.Write16(ADDR_SFX_TRIGGER, 0xABAB);
        }

        private enum SFXType : ushort
        {
            Hurt    = 0x001D,
            Laugh   = 0x0002,
            Holy    = 0x0003,
            PowerUp = 0x0005,
            Explode = 0x0006,
            Death   = 0x0008,
            Rescue  = 0x0009,
            Pickup  = 0x000E,
            Door    = 0x0010,
            Buzz    = 0x0013,
            Roar    = 0x0022,
            Scream  = 0x0024,
            Twinkle = 0x0031
        }
    }
}
