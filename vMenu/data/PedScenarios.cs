using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMenuClient
{
    class PedScenarios
    {
        public static List<string> PositionBasedScenarios = new List<string>()
        {
            "PROP_HUMAN_SEAT_ARMCHAIR",
            "PROP_HUMAN_SEAT_BAR",
            "PROP_HUMAN_SEAT_BENCH",
            "PROP_HUMAN_SEAT_BUS_STOP_WAIT",
            "PROP_HUMAN_SEAT_CHAIR",
            "PROP_HUMAN_SEAT_CHAIR_UPRIGHT",
            "PROP_HUMAN_SEAT_CHAIR_MP_PLAYER",
            "PROP_HUMAN_SEAT_COMPUTER",
            "PROP_HUMAN_SEAT_DECKCHAIR",
            //"PROP_HUMAN_SEAT_DECKCHAIR_DRINK",
            //"PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS",
            //"PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS_PRISON",
            "PROP_HUMAN_SEAT_STRIP_WATCH",
            "PROP_HUMAN_SEAT_SUNLOUNGER",
            "WORLD_HUMAN_SEAT_LEDGE",
            "WORLD_HUMAN_SEAT_STEPS",
            "WORLD_HUMAN_SEAT_WALL",
        };

        /// <summary>
        /// All scenario names (readable version) linked to the actual scenario strings (code names).
        /// </summary>
        public static Dictionary<string, string> ScenarioNames = new Dictionary<string, string>
        {
            ["Выпить кофе"] = "WORLD_HUMAN_AA_COFFEE",
            ["Курить сигарету"] = "WORLD_HUMAN_AA_SMOKE",
            ["Смотреть в бинокль"] = "WORLD_HUMAN_BINOCULARS",
            ["Стоять с табличкой"] = "WORLD_HUMAN_BUM_FREEWAY",
            ["Расслабится лёжа"] = "WORLD_HUMAN_BUM_SLUMPED",
            ["Стоять в ожидании"] = "WORLD_HUMAN_BUM_STANDING",
            ["Осмотреть землю"] = "WORLD_HUMAN_BUM_WASH",
            ["Контролировать траффик"] = "WORLD_HUMAN_CAR_PARK_ATTENDANT",
            ["Радоваться"] = "WORLD_HUMAN_CHEERING",
            ["Стоять с планшетом"] = "WORLD_HUMAN_CLIPBOARD",
            ["Работать отбойником"] = "WORLD_HUMAN_CONST_DRILL",
            ["Стоять в ожидании (COP)"] = "WORLD_HUMAN_COP_IDLES",
            ["Выпить пива"] = "WORLD_HUMAN_DRINKING",
            ["Наркодилер"] = "WORLD_HUMAN_DRUG_DEALER",
            ["Наркодилер_2"] = "WORLD_HUMAN_DRUG_DEALER_HARD",
            ["Фотографировать на телефон"] = "WORLD_HUMAN_MOBILE_FILM_SHOCKING",
            ["Уборка листьев"] = "WORLD_HUMAN_GARDENER_LEAF_BLOWER",
            ["Садить растение"] = "WORLD_HUMAN_GARDENER_PLANT",
            ["Играть в гольф"] = "WORLD_HUMAN_GOLF_PLAYER",
            ["Стоять и осматриваться"] = "WORLD_HUMAN_GUARD_PATROL",
            ["Стоять в ожидании (Охранник)"] = "WORLD_HUMAN_GUARD_STAND",
            ["Работать молотком"] = "WORLD_HUMAN_HAMMERING",
            ["Стоять и разговаривать"] = "WORLD_HUMAN_HANG_OUT_STREET",
            ["Стоять с рюкзаком"] = "WORLD_HUMAN_HIKER_STANDING",
            ["Статуя"] = "WORLD_HUMAN_HUMAN_STATUE",
            ["Стоять с метлой"] = "WORLD_HUMAN_JANITOR",
            ["Бежать на месте"] = "WORLD_HUMAN_JOG_STANDING",
            ["Опереться"] = "WORLD_HUMAN_LEANING",
            ["Протирать тряпкой"] = "WORLD_HUMAN_MAID_CLEAN",
            ["Показывать мускулы"] = "WORLD_HUMAN_MUSCLE_FLEX",
            ["Поднимать штангу (Стоя)"] = "WORLD_HUMAN_MUSCLE_FREE_WEIGHTS",
            ["Играть музыку"] = "WORLD_HUMAN_MUSICIAN",
            ["Фотографировать"] = "WORLD_HUMAN_PAPARAZZI",
            ["Веселится"] = "WORLD_HUMAN_PARTYING",
            ["Присесть на пикнике"] = "WORLD_HUMAN_PICNIC",
            ["Хукер"] = "WORLD_HUMAN_PROSTITUTE_HIGH_CLASS",
            ["Хукер_2"] = "WORLD_HUMAN_PROSTITUTE_LOW_CLASS",
            ["Отжиматься"] = "WORLD_HUMAN_PUSH_UPS",
            ["Сесть"] = "WORLD_HUMAN_SEAT_LEDGE", //
            ["Сесть_2"] = "WORLD_HUMAN_SEAT_STEPS", //
            ["Сесть_3"] = "WORLD_HUMAN_SEAT_WALL", // 
            ["Фонарик"] = "WORLD_HUMAN_SECURITY_SHINE_TORCH",
            ["Пресс"] = "WORLD_HUMAN_SIT_UPS",
            ["Курить"] = "WORLD_HUMAN_SMOKING",
            ["Курить_2"] = "WORLD_HUMAN_SMOKING_POT",
            ["Стоять у огня"] = "WORLD_HUMAN_STAND_FIRE",
            ["Рыбачить"] = "WORLD_HUMAN_STAND_FISHING",
            ["Стоять"] = "WORLD_HUMAN_STAND_IMPATIENT",
            ["Стоять_1"] = "WORLD_HUMAN_STAND_IMPATIENT_UPRIGHT",
            ["Телефон"] = "WORLD_HUMAN_STAND_MOBILE",
            ["Телефон_1"] = "WORLD_HUMAN_STAND_MOBILE_UPRIGHT",  // Copy ["Телефон"] = "WORLD_HUMAN_STAND_MOBILE",
            ["Подтанцовывать"] = "WORLD_HUMAN_STRIP_WATCH_STAND",
            ["Лежать на земле"] = "WORLD_HUMAN_STUPOR",
            ["Загорать"] = "WORLD_HUMAN_SUNBATHE",
            ["Загорать на спине"] = "WORLD_HUMAN_SUNBATHE_BACK",
            ["Играть в теннис"] = "WORLD_HUMAN_TENNIS_PLAYER",
            ["Карта"] = "WORLD_HUMAN_TOURIST_MAP",
            ["Карта в телефоне"] = "WORLD_HUMAN_TOURIST_MOBILE",
            ["Механик"] = "WORLD_HUMAN_VEHICLE_MECHANIC",
            ["Сварка"] = "WORLD_HUMAN_WELDING",
            ["Осматривать"] = "WORLD_HUMAN_WINDOW_SHOP_BROWSE",
            ["Йога"] = "WORLD_HUMAN_YOGA",
            ["Банкомат"] = "PROP_HUMAN_ATM",
            ["BBQ"] = "PROP_HUMAN_BBQ",
            ["Поиск"] = "PROP_HUMAN_BUM_BIN",
            ["Ждать оперевевшись"] = "PROP_HUMAN_BUM_SHOPPING_CART",
            ["Подтягиваться"] = "PROP_HUMAN_MUSCLE_CHIN_UPS",
            ["Подтягиваться_1"] = "PROP_HUMAN_MUSCLE_CHIN_UPS_ARMY", // Copy ["Подтягиваться"] = "PROP_HUMAN_MUSCLE_CHIN_UPS",
            ["Подтягиваться_2"] = "PROP_HUMAN_MUSCLE_CHIN_UPS_PRISON", // Copy ["Подтягиваться"] = "PROP_HUMAN_MUSCLE_CHIN_UPS",
            ["Платить за паркинг"] = "PROP_HUMAN_PARKING_METER",
            ["Сесть на стул"] = "PROP_HUMAN_SEAT_ARMCHAIR", //
            ["Сесть в баре"] = "PROP_HUMAN_SEAT_BAR", //
            ["Сесть на стул_2"] = "PROP_HUMAN_SEAT_BENCH", // Copy ["Сесть на стул"] = "PROP_HUMAN_SEAT_ARMCHAIR", 
            ["Сесть на стул_3"] = "PROP_HUMAN_SEAT_BUS_STOP_WAIT", // Copy ["Сесть на стул"] = "PROP_HUMAN_SEAT_ARMCHAIR",
            ["Сесть на стул_4"] = "PROP_HUMAN_SEAT_CHAIR", // Copy ["Сесть на стул"] = "PROP_HUMAN_SEAT_ARMCHAIR",
            ["Сесть на стул_5"] = "PROP_HUMAN_SEAT_CHAIR_UPRIGHT", // Всегда будет сидеть ровно :/
            ["Сесть MP"] = "PROP_HUMAN_SEAT_CHAIR_MP_PLAYER", //
            ["Сесть за компьютер"] = "PROP_HUMAN_SEAT_COMPUTER", // Error
            ["Разлечься"] = "PROP_HUMAN_SEAT_DECKCHAIR", // 
            ["Разлечься с напитком"] = "PROP_HUMAN_SEAT_DECKCHAIR_DRINK", // 
            ["Поднимать штангу лёжа"] = "PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS", //
            ["Поднимать штангу лёжа_2"] = "PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS_PRISON", // Copy  ["Поднимать штангу лёжа"] = "PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS",
            ["Сидеть и смотреть"] = "PROP_HUMAN_SEAT_STRIP_WATCH", //
            ["Сесть на шезлонг"] = "PROP_HUMAN_SEAT_SUNLOUNGER", //
            ["Стоять_3"] = "PROP_HUMAN_STAND_IMPATIENT", // Копия чего-то :/
            ["Стоять у дороги"] = "CODE_HUMAN_CROSS_ROAD_WAIT",
            ["Осматривать на земле"] = "CODE_HUMAN_MEDIC_KNEEL",
            ["Осматривать тело"] = "CODE_HUMAN_MEDIC_TEND_TO_DEAD",
            ["Блокнот"] = "CODE_HUMAN_MEDIC_TIME_OF_DEATH",
            ["Стоять_4"] = "CODE_HUMAN_POLICE_CROWD_CONTROL", // wtf
            ["Осматривать место"] = "CODE_HUMAN_POLICE_INVESTIGATE",
        };

        /// <summary>
        /// A list containing all readable strings.
        /// </summary>
        public static List<string> Scenarios = new List<string>
        {
            "Выпить кофе",
            "Курить сигарету",
            "Смотреть в бинокль",
            "Стоять с табличкой",
            "Расслабится лёжа",
            "Стоять в ожидании",
            "Осмотреть землю",
            "Контролировать траффик",
            "Радоваться",
            "Стоять с планшетом",
            "Работать отбойником",
            "Стоять в ожидании (COP)",
            "Выпить пива",
            "Наркодилер",
            "Наркодилер_2",
            "Фотографировать на телефон",
            "Уборка листьев",
            "Садить растение",
            "Играть в гольф",
            "Стоять и осматриваться",
            "Стоять в ожидании (Охранник)",
            "Работать молотком",
            "Стоять и разговаривать",
            "Стоять с рюкзаком",
            "Статуя",
            "Стоять с метлой",
            "Бежать на месте",
            "Опереться",
            "Протирать тряпкой",
            "Показывать мускулы",
            "Поднимать штангу (Стоя)",
            "Играть музыку",
            "Фотографировать",
            "Веселится",
            "Присесть на пикнике",
            "Хукер",
            "Хукер_2",
            "Отжиматься",
            "Сесть",
            "Сесть_2",
            "Сесть_3",
            "Фонарик",
            "Пресс",
            "Курить",
            "Курить_2",
            "Стоять у огня",
            "Рыбачить",
            "Стоять",
            "Стоять_1",
            "Телефон",
            "Телефон_1",
            "Подтанцовывать",
            "Сидеть на земле",
            "Загорать",
            "Загорать на спине",
            "Играть в теннис",
            "Карта",
            "Карта в телефоне",
            "Механик",
            "Сварка",
            "Осматривать",
            "Йога",
            "Банкомат",
            "BBQ",
            "Поиск",
            "Ждать оперевевшись",
            "Подтягиваться",
            "Подтягиваться_1",
            "Подтягиваться_2",
            "Платить за паркинг",
            "Сесть на стул",
            "Сесть в баре",
            "Сесть на стул_2",
            "Сесть на стул_3",
            "Сесть на стул_4",
            "Сесть на стул_5",
            "Сесть MP",
            "Сесть за компьютер",
            "Разлечься",
            "Разлечься с напитком",
            "Поднимать штангу лёжа",
            "Поднимать штангу лёжа_2",
            "Сидеть и смотреть",
            "Сесть на шезлонг",
            "Стоять_3",
            "Стоять у дороги",
            "Осматривать на земле",
            "Осматривать тело",
            "Блокнот",
            "Стоять_4",
            "Осматривать место",
        };
    }
}
