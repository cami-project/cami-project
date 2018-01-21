using System;
using System.Collections.Generic;

namespace DSS.Rules.Library
{
    public class Loc
    {

        public static string MSG = "MSG", DES = "DES", EN = "EN", RO = "RO", PL = "PL", DK = "DK", CAREGVR= "CAREGVR", USR = "USR";


        public static string STEPS_LESS_1000 = "STEPS_LESS_1000";
        public static string STEPS_BETWEEN_1000_2000 = "STEPS_BETWEEN_1000_2000";
        public static string STEPS_BIGGER_6000 = "STEPS_BIGGER_6000";

        public static string REMINDER_SENT = "REMINDER_SENT";
        public static string REMINDER_POSTPONED = "REMINDER_POSTPONED";
        public static string REMINDER_IGNORED = "REMINDER_IGNORED";
        public static string MEASUREMENT_IGNORED = "MEASUREMENT_IGNORED";

        public static string WEIGHT_DEC = "WEIGHT_DEC";
        public static string WEIGHT_INC = "WEIGHT_INC";

        public static string PULSE_LOW = "PULSE_LOW";
        public static string PULSE_MID_LOW = "PULSE_MID_LOW";
        public static string PULSE_MEDIUM = "PULSE_MEDIUM";
        public static string PULSE_HIGH = "PULSE_HIGH";

        public static string EXERCISE_STARTED = "EXERCISE_STARTED";
        public static string EXERCISE_ENDED = "EXERCISE_ENDED";
        public static string EXERCISE_ENDED_LOW = "EXERCISE_ENDED_LOW";
        public static string EXERCISE_ENDED_MID = "EXERCISE_ENDED_MID";
        public static string EXERCISE_ENDED_HIGH = "EXERCISE_ENDED_HIGH";

        public static string FALL = "FALL";


        private static Dictionary<string, string> text = new Dictionary<string, string>(){

            //STEPS
            { EN + MSG + STEPS_LESS_1000, "Low step count alert!" },
            { EN + DES + STEPS_LESS_1000, "The person under your care has walked very few steps today. Call them to ask they move more." },
            { PL + MSG + STEPS_LESS_1000, "Alert o małej liczbie kroków!" },
            { PL + DES + STEPS_LESS_1000, "Osoba, którą się opiekujesz, zrobiła dziś niewielką liczbę kroków. Skontaktuj się z nią i poproś, żeby zrobiła ich więcej." },
            { RO + MSG + STEPS_LESS_1000, "Alerta numar de pasi scazut!" },
            { RO + DES + STEPS_LESS_1000, "Persoana in grija dvs. a facut un numar scazut de pasi astazi. Sunati pentru a o ruga sa fie mai activa." },
            { DK + MSG + STEPS_LESS_1000, "Alarm - få skridt registreret!" },
            { DK + DES + STEPS_LESS_1000, "Vi har registreret et lavt aktivitetsniveau. Tag venligst kontakt og spørg ind." },

            { EN + MSG + STEPS_BETWEEN_1000_2000, "Hey! Your number of steps for today is quite low: {0}" },
            { EN + DES + STEPS_BETWEEN_1000_2000, "Why not take a short walk?" },
            { PL + MSG + STEPS_BETWEEN_1000_2000, "Hej! Liczba kroków, które dziś zrobiłeś, jest dość mała: {0}" },
            { PL + DES + STEPS_BETWEEN_1000_2000, "Co powiesz na krótki spacer?" },
            { RO + MSG + STEPS_BETWEEN_1000_2000, "Buna! Numarul tau de pasi de azi e destul de scazut: {0}" },
            { RO + DES + STEPS_BETWEEN_1000_2000, "Ce-ai zice de o scurta plimbare data viitoare?" },
            { DK + MSG + STEPS_BETWEEN_1000_2000, "Hej. Antallet af skridt for i dag er temmeligt lavt: {0}" },
            { DK + DES + STEPS_BETWEEN_1000_2000, "Hvorfor tager du ikke en kort gåtur ved lejlighed?" },

            { EN + MSG + STEPS_BIGGER_6000, "Hey! Good job, today you made {0} steps." },
            { EN + DES + STEPS_BIGGER_6000, "Today you made {0} steps! Keep it up!" },
            { PL + MSG + STEPS_BIGGER_6000, "Hej! Brawo! Dziś zrobiłeś {0} kroków" },
            { PL + DES + STEPS_BIGGER_6000, "Dziś zrobiłeś {0} kroków! Tak trzymaj" },
            { RO + MSG + STEPS_BIGGER_6000, "Buna! Foarte bine, azi ai facut {0} pasi." },
            { RO + DES + STEPS_BIGGER_6000, "Azi ai facut {0} pasi! Tine-o tot asa!" },
            { DK + MSG + STEPS_BIGGER_6000, "Hey! Good job, today you made {0} steps." }, //MISSING 
            { DK + DES + STEPS_BIGGER_6000, "Hej. Godt gået, i dag har du gået  {0} skridt." },
             
            //BLOOD PRESSURE - REMINDER SENT
            { EN + USR + MSG + REMINDER_SENT, "Time for your morning blood pressure measurement!" },
            { EN + USR + DES + REMINDER_SENT, "Please take your blood pressure before breakfast." },
            { EN + CAREGVR + MSG + REMINDER_SENT, "Reminder for morning blood pressure measurement sent!" },
            { EN + CAREGVR + DES + REMINDER_SENT, "Check on the person under your care to see that he took the recommended blood pressure measurement." },

            { PL + USR + MSG + REMINDER_SENT, "Czas na Twój poranny pomiar ciśnienia!" },
            { PL + USR + DES + REMINDER_SENT, "Zmierz ciśnienie zanim zjesz śniadanie." },
            { PL + CAREGVR + MSG + REMINDER_SENT, "Wysłano powiadomienie o porannym pomiarze ciśnienia!" },
            { PL + CAREGVR + DES + REMINDER_SENT, "Sprawdź, czy osoba, którą się opiekujesz, wykonała pomiar ciśnienia. " },

            { RO + USR + MSG + REMINDER_SENT, "E timpul pentru a-ti masura tensiunea de dimineata!" },
            { RO + USR + DES + REMINDER_SENT, "Te rugam sa-ti masori tensiunea inainte de micul dejun." },
            { RO + CAREGVR + MSG + REMINDER_SENT, "Notificare trimisa pentru masurarea tensiunii de dimineata!" },
            { RO + CAREGVR + DES + REMINDER_SENT, "Verificati ca Persoana in grija dvs. sa efectueze masuratoarea de tensiune." },

            { DK + USR + MSG + REMINDER_SENT, "Det er tid til at tage din morgenmåling af blodtrykket!" },
            { DK + USR + DES + REMINDER_SENT, "Husk venligst at tage dit blodtryk før morgenmaden." },
            { DK + CAREGVR + MSG + REMINDER_SENT, "Påmindelse for morgenmåling af blodtryk er sendt!" },
            { DK + CAREGVR + DES + REMINDER_SENT, "Følg venligst op på om der blev taget en blodtryksmåling." },


            //REMINDER-SNOOZED
            { EN + CAREGVR + MSG + REMINDER_POSTPONED, "The person under your care postponed the blood pressure measurements." },
            { EN + CAREGVR + DES + REMINDER_POSTPONED, "Please take action and call to remind them of the measurement." },
            { PL + CAREGVR + MSG + REMINDER_POSTPONED, "Osoba, którą się opiekujesz, przełożyła pomiar ciśnienia na później." },
            { PL + CAREGVR + DES + REMINDER_POSTPONED, "Skontaktuj się z nią i przypomnij o pomiarze." },
            { RO + CAREGVR + MSG + REMINDER_POSTPONED, "Persoana in grija dvs. a amanat masuratoarea de tensiune." },
            { RO + CAREGVR + DES + REMINDER_POSTPONED, "Va rugam sa sunati pentru a-i aduce aminte de masuratoare." },
            { DK + CAREGVR + MSG + REMINDER_POSTPONED, "Blodtryksmålingen blev udsat til et senere tidspunkt." },
            { DK + CAREGVR + DES + REMINDER_POSTPONED, "Vil du venligst ringe og give en påmindelse om målingen?" },


            //REMINDER IGNORED
            { EN + CAREGVR + MSG + REMINDER_IGNORED, "The person under your care ignored the reminder for blood pressure measurement." },
            { EN + CAREGVR + DES + REMINDER_IGNORED, "Please take action and call to remind them of the measurement." },
            { PL + CAREGVR + MSG + REMINDER_IGNORED, "Osoba, którą się opiekujesz, zignorowała przypomnienie o pomiarze ciśnienia." },
            { PL + CAREGVR + DES + REMINDER_IGNORED, "Skontaktuj się z nią i przypomnij o pomiarze." },
            { RO + CAREGVR + MSG + REMINDER_IGNORED, "Persoana in grija dvs. a ignorat notificarea pentru masurarea tensiunii." },
            { RO + CAREGVR + DES + REMINDER_IGNORED, "Va rugam sa sunati pentru a-i aduce aminte de masuratoare." },
            { DK + CAREGVR + MSG + REMINDER_IGNORED, "Påmindelsen om at tage blodtryksmålingen blev ignoreret." },
            { DK + CAREGVR + DES + REMINDER_IGNORED, "Vil du være så venlig at ringe og give en påmindelse om målingen?" },


            //MEASUREMENT IGNORED
            { EN + CAREGVR + MSG + MEASUREMENT_IGNORED, "The person under your care did not take the blood pressure measurement." },
            { EN + CAREGVR + DES + MEASUREMENT_IGNORED, "Please take action and call to remind them of the measurement." },
            { PL + CAREGVR + MSG + MEASUREMENT_IGNORED, "Osoba, którą się opiekujesz, nie zmierzyła ciśnienia." },
            { PL + CAREGVR + DES + MEASUREMENT_IGNORED, "Skontaktuj się z nią i przypomnij o pomiarze" },
            { RO + CAREGVR + MSG + MEASUREMENT_IGNORED, "Persoana in grija dvs. nu a efectuat masuratoarea de tensiune." },
            { RO + CAREGVR + DES + MEASUREMENT_IGNORED, "Va rugam interveniti si sunati pentru a-i aduce aminte de acest lucru." },
            { DK + CAREGVR + MSG + MEASUREMENT_IGNORED, "Blodtryksmålingen blev ikke taget" },
            { DK + CAREGVR + DES + MEASUREMENT_IGNORED, "Vil du være så venlig at ringe og give en påmindelse om målingen?" },

            //WEIGHT ANALYSIS - DEC
            { EN + USR + MSG + WEIGHT_DEC, "There's a decrease of {0} kg in your weight." },
            { EN + USR + DES + WEIGHT_DEC, "Please take your meals regularly." },
            { EN + CAREGVR + MSG + WEIGHT_DEC, "The person under your care lost {0} kg." },
            { EN + CAREGVR + DES + WEIGHT_DEC, "You can contact him and see what's wrong." },

            { PL + USR + MSG + WEIGHT_DEC, "Twoja waga spadła o {0} kg" },
            { PL + USR + DES + WEIGHT_DEC, "Pamiętaj o regularnym spożywaniu posiłków" },
            { PL + CAREGVR + MSG + WEIGHT_DEC, "Waga osoby, którą się opiekujesz, spadła o {0} kg" },
            { PL + CAREGVR + DES + WEIGHT_DEC, "Możesz się z nią skontaktować i dowiedzieć się, co się dzieje. " },

            { RO + USR + MSG + WEIGHT_DEC, "Ai scazut {0} kg in greutate" },
            { RO + USR + DES + WEIGHT_DEC, "Te rugam sa mananci regulat." },
            { RO + CAREGVR + MSG + WEIGHT_DEC, "Persoana in grija dvs. a slabit {0} kg!" },
            { RO + CAREGVR + DES + WEIGHT_DEC, "Va rugam contactati-o pentru a afla ce se intampla." },

            { DK + USR + MSG + WEIGHT_DEC, "Du har tabt dig {0} kg i vægt." },
            { DK + USR + DES + WEIGHT_DEC, "Huske at spise med jævne mellemrum." },
            { DK + CAREGVR + MSG + WEIGHT_DEC, "Personen under din omsorg har tabt {0} kg." },
            { DK + CAREGVR + DES + WEIGHT_DEC, "Du kan eventuelt kontakte vedkommende og høre hvad der er galt?" },

            // INC
            { EN + USR + MSG + WEIGHT_INC, "There's a increase of {0} kg in your weight." },
            { EN + USR + DES + WEIGHT_INC, "Please be careful with your meals." },
            { EN + CAREGVR + MSG + WEIGHT_INC, "The person under your care gained {0} kg." },
            { EN + CAREGVR + DES + WEIGHT_INC, "Please check if this has to do with his diet." },

            { PL + USR + MSG + WEIGHT_INC, "Twoja waga wzrosła o {0} kg." },
            { PL + USR + DES + WEIGHT_INC, "Zwróć uwagę na swoje posiłki." },
            { PL + CAREGVR + MSG + WEIGHT_INC, "Waga osoby, którą się opiekujesz, wzrosła o {0} kg" },
            { PL + CAREGVR + DES + WEIGHT_INC, "Sprawdź, czy to ma związek z jej dietą" },

            { RO + USR + MSG + WEIGHT_INC, "Ai luat {0} kg in greutate." },
            { RO + USR + DES + WEIGHT_INC, "Te rugam sa ai grija cu mesele." },
            { RO + CAREGVR + MSG + WEIGHT_INC, "Persoana in grija dvs. a luat {0} kg in greutate." },
            { RO + CAREGVR + DES + WEIGHT_INC, "Va rugam verificati daca este din cauza dietei." },

            { DK + USR + MSG + WEIGHT_INC, "Du har taget {0} kg på i væg" },
            { DK + USR + DES + WEIGHT_INC, "Vær venligst påpasselig med dine målmider." },
            { DK + CAREGVR + MSG + WEIGHT_INC, "Personen i din pleje har taget {0} kg på." },
            { DK + CAREGVR + DES + WEIGHT_INC, "Undersøg venligst om det har noget med diæten at gøre." },

            //PULSE LOW
            { EN + USR + MSG + PULSE_LOW, "Hey! Your heart rate is quite low: {0}." },
            { EN + USR + DES + PULSE_LOW, "I have contacted your caregiver." },
            { EN + CAREGVR + MSG + PULSE_LOW, "The heart rate of the person under your cares is dangerously low: only {0}!" },
            { EN + CAREGVR + DES + PULSE_LOW, "Please take action now!" },

            { PL + USR + MSG + PULSE_LOW, "Hej! Twoje tętno jest dość niskie: {0}." },
            { PL + USR + DES + PULSE_LOW, "Skontaktowałem się z Twoim opiekunem" },
            { PL + CAREGVR + MSG + PULSE_LOW, "Tętno osoby, którą się opiekujesz, jest niebezpiecznie niskie: tylko {0}" },
            { PL + CAREGVR + DES + PULSE_LOW, "Zareaguj jak najszybciej! " },

            { RO + USR + MSG + PULSE_LOW, "Buna! Pulsul tau este destul de scazut: {0}." },
            { RO + USR + DES + PULSE_LOW, "Am notificat persoanele tale de contact." },
            { RO + CAREGVR + MSG + PULSE_LOW, "Persoana in grija dvs. are pulsul extem de scazut: doar {0}!" },
            { RO + CAREGVR + DES + PULSE_LOW, "Va rugam interveniti acum!" },

            { DK + USR + MSG + PULSE_LOW, "Hej! Din hjerterytme er temmelig lav: {0}." },
            { DK + USR + DES + PULSE_LOW, "Jeg har kontaktet din omsorgsperson" },
            { DK + CAREGVR + MSG + PULSE_LOW, "Personen i din pleje har en meget lav puls: {0}!" },
            { DK + CAREGVR + DES + PULSE_LOW, "Du bør gøre noget snarest muligt!" },

            //PULSE MID_LOW
            { EN + USR + MSG + PULSE_MID_LOW, "Hey! Your heart rate is just a bit low: {0}." },
            { EN + USR + DES + PULSE_MID_LOW, "How about some exercise?" },
            { EN + CAREGVR + MSG + PULSE_MID_LOW, "The heart rate of the person under your care a bit low: only {0}!" },
            { EN + CAREGVR + DES + PULSE_MID_LOW, "Please make sure they are all right." },

            { PL + USR + MSG + PULSE_MID_LOW, "Hej! Twoje tętno jest trochę za niskie: {0}." },
            { PL + USR + DES + PULSE_MID_LOW, "Co powiesz na odrobinę ruchu?" },
            { PL + CAREGVR + MSG + PULSE_MID_LOW, "Tętno osoby, którą się opiekujesz, jest trochę za niskie: {0}!" },
            { PL + CAREGVR + DES + PULSE_MID_LOW, "Sprawdź, czy wszystko u niej w porządku." },

            { RO + USR + MSG + PULSE_MID_LOW, "Buna! Pulsul tau este putin scazut: {0}." },
            { RO + USR + DES + PULSE_MID_LOW, "Ce ai zice de putina miscare?" },
            { RO + CAREGVR + MSG + PULSE_MID_LOW, "Persoana in grija dvs. are pulsul putin scazut: doar {0}!" },
            { RO + CAREGVR + DES + PULSE_MID_LOW, "Va rugam verificati ca este in regula." },

            { DK + USR + MSG + PULSE_MID_LOW, "Hej! Din puls er lidt lav: {0}." },
            { DK + USR + DES + PULSE_MID_LOW, "Hvad med lidt motion?" },
            { DK + CAREGVR + MSG + PULSE_MID_LOW, "Personen i din pleje har en lidt lav puls: {0}!" },
            { DK + CAREGVR + DES + PULSE_MID_LOW, "Vær sød at undersøge om vedkommende er ok." },


            //PULSE MEDIUM
            { EN + USR + MSG + PULSE_MEDIUM, "Hey! Your heart rate is just a bit high:  {0}." },
            { EN + USR + DES + PULSE_MEDIUM, "Why not rest for a bit" },
            { EN + CAREGVR + MSG + PULSE_MEDIUM, "The heart rate of the person under your care is a bit high: over {0}!" },
            { EN + CAREGVR + DES + PULSE_MEDIUM, "Please make sure they are all right." },

            { PL + USR + MSG + PULSE_MEDIUM, " Hej! Twoje tętno jest trochę za wysokie:  {0}." },
            { PL + USR + DES + PULSE_MEDIUM, "Co powiesz na mały odpoczynek?" },
            { PL + CAREGVR + MSG + PULSE_MEDIUM, "Tętno osoby, która się opiekujesz, jest trochę za wysokie: {0}!" },
            { PL + CAREGVR + DES + PULSE_MEDIUM, "Sprawdź, czy u niej wszystko w porządku." },

            { RO + USR + MSG + PULSE_MEDIUM, "Buna! Pulsul tau este putin ridicat:  {0}." },
            { RO + USR + DES + PULSE_MEDIUM, "Ce ai zice de putina odihna?" },
            { RO + CAREGVR + MSG + PULSE_MEDIUM, "Persoana in grija dvs. are pulsul putin ridicat: peste  {0}!" },
            { RO + CAREGVR + DES + PULSE_MEDIUM, "Va rugam verificati ca este in regula." },

            { DK + USR + MSG + PULSE_MEDIUM, "Hej! Din puls er lidt til den høje side:  {0}." },
            { DK + USR + DES + PULSE_MEDIUM, "Måske du skulle hvile lidt?" },
            { DK + CAREGVR + MSG + PULSE_MEDIUM, "Personen i din pleje har en let forhøjet puls: {0}!" },
            { DK + CAREGVR + DES + PULSE_MEDIUM, "Vær sød at undersøge om vedkommende er ok." },


            //HIGH
            { EN + USR + MSG + PULSE_HIGH, "Hey! Your heart rate is quite high:  {0}." },
            { EN + USR + DES + PULSE_HIGH, "I have contacted your caregiver." },
            { EN + CAREGVR + MSG + PULSE_HIGH, "The heart rate of the person under your care is dangerously high: over {0}!" },
            { EN + CAREGVR + DES + PULSE_HIGH, "Please take action now!" },

            { PL + USR + MSG + PULSE_HIGH, "Hej! Twoje tętno jest dość wysokie:  {0}." },
            { PL + USR + DES + PULSE_HIGH, "Skontaktowałem się z Twoim opiekunem" },
            { PL + CAREGVR + MSG + PULSE_HIGH, "Tętno osoby, którą się opiekujesz, jest niebezpiecznie wysokie: aż {0}!" },
            { PL + CAREGVR + DES + PULSE_HIGH, "Zareaguj jak najszybciej! " },

            { RO + USR + MSG + PULSE_HIGH, "Buna! Pulsul tau este destul de ridicat:  {0}." },
            { RO + USR + DES + PULSE_HIGH, "Am notificat persoanele tale de contact." },
            { RO + CAREGVR + MSG + PULSE_HIGH, "Persoana in grija dvs. are pulsul extrem de ridicat: peste {0}!" },
            { RO + CAREGVR + DES + PULSE_HIGH, "Va rugam interveniti acum!" },

            { DK + USR + MSG + PULSE_HIGH, "Hej! Din puls er meget høj:  {0}." },
            { DK + USR + DES + PULSE_HIGH, "Jeg har taget kontakt til din omsorgsperson" },
            { DK + CAREGVR + MSG + PULSE_HIGH, "Hej! Din puls er farligt høj:  {0}!" },
            { DK + CAREGVR + DES + PULSE_HIGH, "Søg hurtigst muligt hjælp!" },


            //EXERCISE 
            {EN + CAREGVR + MSG + EXERCISE_STARTED, "Physical Exercise Started"},
            {EN + CAREGVR + DES + EXERCISE_STARTED, "The person under your care has started a physical exercise of type {0}"},

            {EN + CAREGVR + MSG + EXERCISE_ENDED, "Physical Exercise Ended"},
            {EN + CAREGVR + DES + EXERCISE_ENDED, "The person under your care has finished the {0} exercise with a compliance of {1}%."},

            {EN + USR + MSG + EXERCISE_ENDED_LOW,  "Physical Exercise Ended"},
            {EN + USR + DES + EXERCISE_ENDED_LOW,  "How about focusing more next time?"},
            {EN + USR + MSG + EXERCISE_ENDED_MID,  "Physical Exercise Ended"},
            {EN + USR + DES + EXERCISE_ENDED_MID,  "Keep improving!"},
            {EN + USR + MSG + EXERCISE_ENDED_HIGH, "Physical Exercise Ended"},
            {EN + USR + DES + EXERCISE_ENDED_HIGH, "You performed well!"},


            { EN + CAREGVR + MSG + FALL, "Fall detected!" },
            { EN + CAREGVR + DES + FALL, "Please take action now!" },

        };

        /// <summary>
        /// Depricated and should be replacted with Msg or Des methods
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="lang">Lang.</param>
        /// <param name="type">Type.</param>
        /// <param name="category">Category.</param>
        /// <param name="who">Who.</param>
        public static string Get(string lang, string type, string category, string who = "")
        {
            return text[lang + who + type + category];
        }

        public static string Msg(string category, string lang, string who = "") {
            
            return text[lang + who + MSG + category];
        }
        public static string Des(string category, string lang, string who = "")
        {
            return text[lang + who + DES + category];
        }
    }
}
