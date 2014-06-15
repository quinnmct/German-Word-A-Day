using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using Microsoft.Phone.Scheduler;
using System.IO.IsolatedStorage;
using Windows.Phone.Speech.Synthesis;
using Windows.Phone.Speech.Recognition;

//Add to top of page for testing:
//#define DEBUG_AGENT

namespace TileTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        PeriodicTask periodicTask;
        string periodicTaskName = "PeriodicAgent";
        SpeechSynthesizer _speakerEN = new SpeechSynthesizer(); //english voice
        SpeechSynthesizer _speakerDE = new SpeechSynthesizer();
        //german voice

        private DateTime lastUpdate;
        List<string> wordList;
        List<string> definitionList;
        List<string> sentenceList;
        List<int> chosenIndices;
        string englishSentence;
        string germanSentence;

        bool dailyChange;

        private bool isTileUpdateInProgress = false;

        //used to pick a random indexed word
        Random rand;

        //delimiter to split sentences for TTS
        string[] delimiter = new string[]{"\n\n"};


        //for now, 'count' is the index of the word used to get word of the day
        int count;

        Color phoneAccent;
        Color textAccent;


        // Constructor
        public MainPage()
        {
            InitializeComponent();
            //(App.Current as App).rateReminder.Notify();



            rand = new Random();

            initLists();

            phoneAccent = (Color)Application.Current.Resources["PhoneAccentColor"];
            textAccent = (Color)Application.Current.Resources["PhoneForegroundColor"];

            //if 'first time opened'
            if (!HasUserSeenIntro())
            {
                definitionText.Text = "German Word A Day";
                //store data to isolated storage
                SaveTileInfo();
            }
            else
            {
                //definitionText.Text = "again";
                GetWordOfTheDay();
            }

            if (englishSentence != null)
            {
                englishButton.Visibility = System.Windows.Visibility.Visible;
            }
            if (germanSentence != null)
            {
                germanButton.Visibility = System.Windows.Visibility.Visible;
            }

                

        }

        private void initLists()
        {
            wordList = new List<String>(new string[] {
                "Guten Morgen","Rindfleisch","Nein", "Danke","Entschuldigung",//5
                "Warum?", "Wie?", "Wer?", "Wo ist?","Gestern","Morgen", "Sekunde","Stunde",//13
                "Können","Sehen","Lachen","Klein","Hässlich","Auf Wiedersehen","Schön","Einfach","Schwierig",//22
                "Guten Tag", "Hallo", "Lecker", "Bier", "Kaffee", "Wien", //28
                "Wasser","Hühner","Fuß","Bein","Kopf","Magen",//34
                "Monat","Eltern","Jahr","Landwirt","Arzt","draußen","Bäcker",//41
                "Berg","Bestattet","Biographie","Brücke","Bruder","Burg","Bürgermeister",//48
                "Chirug","Dienst","Dorf","Drei","Ehemann","Eheschließung","Einwohner",//55
                "Eisenbahn","Eltern","Enkelkind","Erbe","Erschien","Familie",//61
                "Festung","Bestätigung","Fleischer","Flüchtling","Fluß","Folgende","Wald",//68
                "Freitag","Fremde","Friedhof","Gärtner","Frau","Gebiet","Geld","Zwillinge","Gericht",//77
                "Geschichte","Gesellschaft","Gesetz","Gestorben","Beruf","Glaube","Gleiche",//84
                "Grenze","Groß","Halb","Haus","Bad","Heimat","Namens","Herkunft",//92
                "Anwesen","Überlebenden","Hoch","Husten","Immer","Innerhalb","Jahreszeit", //99
                "Jeder","Jugend","Karte","Kaufen","Keller","Kind","Kirche","Knecht",//107
                "Kolonist","König","Krämer","Krankheit","Krieg","Land","Leben","Lehrer",//115
                "Leiche","Frühling","Letzter","Mädchen","Maler","Markt","Militär","Mit",//124
                "Mittag","Mitternacht","Montag","Mutter","Nachbar","Neffe","Neu","Nichts",//133
                "Nieder","Noch","Nord","Nummer","Oft","Ohne","Onkel","Ort","Osten","Ostern","Pest",//144
                "Pflegekind","Platz","Priester","Prinz","Rechnung","Rechsanwalt","Reich","Witwe",//152
                "Rentner","Registriert","Schäfer","Mitarbeiter","Friseur","Schiffbauer",//160
                "Schiffer","Schule","Schwarz","Schwester","Sechs","See","Seelen","Seite","Sollten",//169
                "Staat","Stadt","Stellen","Straße","Süden","Tante","Großeltern","Tausend","Tochter",//178
                "Überleben","Unter","uneheliche","Unterschrift","Vergangen","Vermieter","Verlobte",//185
                "Durchführen","Verwandten","Vielleicht","Vor","Vorherig","Vorstadt","Waise","Wegen",//194
                "Weiß","Welche","Wirt","Woche","Wohnen","Wollen","Wörterbuch","Würdig","Zählen",//203
                "Zehn","Zeit","Zeuge","Zimmermann","Zukunft","Zusammen","Zwanzig","Zwischen"//212
            });
            definitionList = new List<string>(new string[]{
                "good morning","beef", "no", "thank you", "sorry",
                "why?", "how?", "who?", "where is?","yesterday","tomorrow", "second", "hour",
                "can","see","laugh","small","ugly","good-bye","beautiful","easy","difficult",
                "good afternoon", "hello", "delicious", "beer", "coffee", "wine", 
                "water","chicken","foot","leg","head","stomach",
                "month","parents","year","farmer","physician, doctor","outside","baker",
                "hill, mountain","buried","biography","bridge","brother","castle","mayor",
                "surgeon","service, employment","village","three","husband","marriage","inhabitant, native",
                "railroad","parents","grandchild","heirs, successors","appeared","family, household",
                "fortress", "confirmation","butcher","refugee, deserter","river","following, next","forest",
                "friday","foreign, strange","cemetary","gardener","wife, woman","region, area, zone","money","twins, pairs","court",
                "history","society, group","law, guideline","died, passed","trade, occupation","belief, faith", "same, alike, similar",
                "border","big, great, large","half","house, home","bath, spa","native place, homeland","is named","place of origin",
                "estate, kingdom","survivors","high, tall","cough","always","inside of","season, specific time period",
                "each, every","youth, adolescence","map","to buy","cellar, basement","baby, toddler","church","servant",
                "settler, colonist, farmer","king","grocer, small retailer","disease, sickness","war","land, country","living","teacher",
                "body, corpse","spring (season)","last, latter","girl","painter, artist","market","military, army","with, via, by",
                "midday, noon","midnight","monday","mother","neighbor","nephew","new","nothing",
                "lower","still, persisting","north","number","often","without","uncle","place, town","east","Easter","plague",
                "foster child, adopted","place, location","priest, ritual leader","prince","account, bill","lawyer","empire, kingdom","widow",
                "retired, resigned","register","shepherd","worker, employee","barber, beardcutter","shipbuilder",
                "sailor, seaman","school","black","sister","six","lake, pond","souls","page","should",
                "state, province","city","to place, to put","street","south","aunt","godparents","thousand","daughter",
                "survive","under","illegitimate","signature","past, previous","landlord, lessor","fiancée",
                "performed, played","relatives, family members","maybe, perhaps","before, ago","previous, preceding","suburbs","orphan","because of",
                "white","which","innkeeper","week","to live, to reside","to want","dictionary","worthy","to count",
                "ten","time","witness","carpenter","future","together","twenty","between"
            });
            sentenceList = new List<string>(new string[]{
                "Guten Morgen, wie hast du geschlafen?\n\nGood morning, how did you sleep?",
                "Was ist das beste Rindfleisch Gericht in Ihrem Restaurant?\n\nWhat is the best beef dish in your restaurant?",
                "Nein, nicht schon wieder die alte Geschichte!\n\nOh no, not that old chestnut again!",
                "Danke Mrs. Färber, war Ihre Rede schön.\n\nThank you Mrs. Färber, your speech was beautiful.",
                "Entschuldigung, ich hielt Sie für  meine Frau!\n\nSorry, I mistook you for my wife!",
                "Warum sollte ich die Schuld auf mich nehmen?\n\nWhy should I take the blame?",
                "Wie wär's mit einem Bierchen?\n\nHow about about beer?",
                "Mich würde interessieren, wer er eigentlich ist.\n\nI wonder who he really is.",
                "Wo ist mein Auto\n\nWhere is my car?",
                "Wir haben gestern Abend einiges getrunken.\n\nWe drank a lot last night.",
                "Morgen geht der Alltagstrott wieder los.\n\nTomorrow it's back to the rat race.",
                "Das Projektil bei 30 Meter pro Sekunde bewegt\n\nThe projectile was moving at 30 meters per second.",
                "Wir werden da sein in einer Stunde\n\nWe will be there in one hour.",
                "Können wir zu den Bars nach der Arbeit gehen\n\nWe can go to the bars after work.",
                "Hast du die Größe des Kürbis\n\nDid you see the size of the pumpkin?",
                "Nicht in anderen Unglück lachen\n\nDont laugh at other's misfortune.",
                "Ihren Strudel war klein aber fein\n\nHer strudel was small but excellent.",
                "Meine Freunde Hund ist hässlich\n\nmy friends dog is ugly",
                "Auf Wiedersehen für den Abend.\n\ngoodbye for the evening.",
                "Ihr Kleid ist schön.\n\nyour dress is beautiful.",
                "Es ist einfach zu erlernen Deutsch\n\nIt is easy to learn German.",
                "Es ist schwierig, Japanisch zu lernen\n\nIt is difficult to learn Japanese",
                "Guten Tag Offizier, was ist das Problem?\n\nGood afternoon officer, what is the problem?",
                "Hallo Sohn, wie ist das College?\n\nHello son, how is college?",
                "Diese Boeuf Stroganoff ist köstlich!\n\nThis beef stroganoff is delicious!",
                "Ich wünschte, ich könnte all das Bier der Welt trinken.\n\nI wish I could drink all the beer in the world.",
                "Ich brauche meinen Kaffee am Morgen.\n\nI need my coffee in the mornings.",
                "Die französisch lieben ihren Wein.\n\nThe French love their wine.",
                "Ich habe Durst, ich brauche Wasser.\n\nI am thirsty, I need water",
                "Mein Großvater besitzt 50 Hühner.\n\nMy grandfather owns 50 chickens.",
                "Setzen Sie Ihren Fuß auf dem Gas!\n\nPut your foot on the gas!",
                "Ich brach mein Bein springen aus dem Fenster\n\nI broke my leg jumping out of the window.",
                "Benutzen Sie Ihren Kopf Dummkopf!\n\nUse your head dummy!",
                "Mein Vater hat einen großen Magen.\n\nMy father has a big stomach",
                "Morgen ist ein neuer Monat\n\nTomorrow is a new month",
                "Verstecken Sie die Süßigkeiten von den Eltern!\n\nHide the candy from your parents!",
                "Ich werde aufhören zu trinken im nächsten Jahr\n\nI will stop drinking next year",
                "Dass Landwirt nicht verkaufen Kartoffeln\n\nThat farmer doesnt sell potatoes",
                "Mein Arzt empfiehlt, von zu Hause aus arbeiten\n\nMy physician recommends working from home",
                "Ich muss nach draußen gehen, um etwas frische Luft zu bekommen\n\nI need to go outside to get some fresh air",
                "Mein Bäcker macht das beste Sauerteigbrot\n\nMy local baker makes the best sourdough bread",
                "Mein Haus ist an der Spitze des Berg.\n\nMy house is on top of this hill.",
                "Der schatz bestattet unter dieser Palme begraben.\n\nThe treasure is buried under that palm tree.",
                "Ich lese die Biographie von Albert Einstein.\n\nI am reading the biography of Albert Einstein.",
                "Der Zug überquert eine lange Brücke.\n\nThe train will cross a long bridge.",
                "Haben Sie etwas dagegen, wenn ich meinen Bruder mitbringen?\n\nDo you mind if I bring my brother along?",
                "Die Armee bleibt in der Burg.\n\nthe army stays in the castle.",
                "Der Schlüssel wurde mir durch den Bürgermeister gegeben.\n\nThe key was given to me by the mayor.",
                "Was ist der Name meiner Mutter Chirurg?\n\nWhat is the name of my mother's surgeon?",
                "Es ist mein Dienst als Feuerwehrmann, Brände zu löschen.\n\nIt is my service as a fireman to put out fires.",
                "Die Siedler das Dorf zerstört.\n\nThe settlers destroyed the village.",
                "Ich möchte drei Pralinen und zwei Torten.\n\nI would like three chocolates and two pies.",
                "Er wird mein Ehemann sein im nächsten Monat.\n\nHe will be my husband next month.",
                "Unsere Ehe war der beste Tag meines Lebens!\n\nOur marriage was the best day of my life!",
                "Die Ureinwohner warfen Speere und Pfeile auf uns.\n\nThe natives threw spears and arrows at us.",
                "Mein Vater half beim Aufbau der Eisenbahn.\n\nMy father helped build the railroad.",
                "Meine Eltern kennengelernt, als sie 16 waren.\n\nMy parents met when they were 16.",
                "Unser ältester Enkel ist 5 Jahre alt.\n\nOur oldest grandchild is 5 years old.",
                "Der älteste Sohn ist der Erbe des Anwesens.\n\nThe oldest son is the heir to the estate.",
                "Ein Komet erschien in den Nachthimmel.\n\nA comet appeared in the night sky.",
                "Ich bin sehr zufrieden mit meiner Familie zu schließen.\n\nI am very close with my family.",
                "Er wird nie seine Festung zu verlassen.\n\nHe will never have to leave his fortress.",
                "Sie benötigen eine Bestätigung von Ihrem Lehrer.\n\nYou need a confirmation from your teacher.",
                "Der Fleischer raucht seine Fleisch.\n\nThe butcher smokes his meats.",
                "Der Feind wird nicht zurückkehren, die Flüchtlinge.\n\nThe enemy won't return the refugees.",
                "Mein Wasser kommt aus dem Fluss.\n\nMy water comes from the river.",
                "Ich mag das folgende Lied besser.\n\nI like the next song better.",
                "Der Wald kommt nachts lebendig.\n\nThe forest comes alive at night.",
                "Ich ging zur Bar Freitag Nacht.\n\nI went to the bar friday night.",
                "Ich fühlte fremde in meinem neuen Zuhause.\n\nI felt foreign in my new home.",
                "Meine Großeltern leben in dem Friedhof jetzt.\n\nMy grandparents live in the cemetary now.",
                "Der Gärtner kam, um die Pflanzen zu gießen.\n\nThe gardener came to water the plants.",
                "Meine Frau liebt die Reinigung des Hauses.\n\nMy wife loves cleaning the house.",
                "Das Gebiet im Süden ist meistens Wüste.\n\nThe region to the south is mostly desert.",
                "Geben Sie Ihr ganzes Geld für wohltätige Zwecke.\n\nGive all your money to charity.",
                "Ich habe immer rufen meine Zwillinge von dem falschen Namen.\n\nI always call my twins by the wrong name.",
                "Der Richter arbeitet in Gericht.\n\nThe judge works in the court.",
                "Geschichte wiederholt sich immer.\n\nHistory always repeats itself.",
                "Gesellschaft blickt auf dieser Gruppe von Menschen.\n\nSociety looks down upon that group of people.",
                "Sie wird in Ordnung sein, wenn man die Gesetze zu befolgen.\n\nYou will be fine if you follow the laws.",
                "Mein Goldfisch gestorben tun, was er am besten liebte.\n\nMy goldfish died doing what he loved best",
                "Sie sollten einen Beruf vor dem Verlassen der Universität holen.\n\nYou should pick a job before leaving college.",
                "Sein Glaube ist der Hinduismus.\n\nHis belief is hinduism.",
                "Das neue Modell geht doppelt so schnell.\n\nThe new model goes twice as fast.",
                "Die Grenze zwischen Koreas wird schwer bewacht.\n\nThe border between Koreas is heavily guarded.",
                "Sie modelliert ein großes Modell von meiner Katze.\n\nShe sculpted a large model of my cat.",
                "Halb und halb ist deins ist meins.\n\nHalf is yours and half is mine.",
                "Mein Haus ist zu klein für meine Familie.\n\nMy house is too small for my family.",
                "Ich brauche ein Bad nach der Arbeit aus.\n\nI need a bath after working out.",
                "Sie werden bis zum Tod für ihre Heimat kämpfen.\n\nThey will fight to the death for their homeland.",
                "Ihre Schaf namens Daisy.\n\nHer sheep is named daisy.",
                "Seine Herkunft ist das Basislager.\n\nHis place of origin is the base camp.",
                "Die Königin verlässt niemals das Anwesen.\n\nThe queen never leaves the estate.",
                "Es gab nur einen Überlebenden\n\nThere was only one survivor.",
                "Sein Zaun ist zu hoch, um zu klettern.\n\nHis fence is too high to climb.",
                "Zigaretten machen mich immer husten.\n\nCigarettes always make me cough.",
                "Die allgemeine immer führen seine Männer in Sicherheit.\n\nThe general will always lead his men to safety.",
                "Haben Sie jemals innerhalb einem Computer gesehen?\n\nHave you ever seen inside of a computer?",
                "Meine liebste Jahreszeit ist Sommer.\n\nMy favorite season is Summer.",
                "Jeder zweite fühlte sich an wie eine Stunde.\n\nEach second felt like an hour.",
                "Die Kinder werden immer ihre Jugend.\n\nThe children will always have their youth.",
                "Ich kann paris auf einer Karte zu finden.\n\nI can find paris on a map.",
                "Ich muss ihn kaufen ein Geburtstagsgeschenk.\n\nI must buy him a birthday present.",
                "Die Stühle sind im Keller.\n\nThe chairs are in the cellar.",
                "Das Kind nicht gerne abgeholt werden.\n\nThe baby did not like to be picked up.",
                "Mein Urgroßvater half beim Aufbau der Kirche.\n\nMy great grandfather helped build the church.",
                "Mein Diener bereiten eine Mahlzeit.\n\nMy servant will prepare a meal.",
                "Die Kolonist forderten freies Land.\n\nThe colonist demanded free land.",
                "Der König verliert seine Krone.\n\nThe king will lose his crown.",
                "Wir müssen an der Krämerladen Karotten bekommen.\n\nWe need to get carrots at the grocer.",
                "Ich hoffe, dass ihre Krankheit geht weg.\n\nI hope her disease goes away.",
                "Die Nomaden waren ständig im Krieg.\n\nThe nomads were constantly at war.",
                "Ich bin auf meinem eigenen Land stehen.\n\nI am standing on my own land.",
                "Wir sind alle auf diesem Planeten leben.\n\nWe are all living on this planet.",
                "Meine Klavierlehrer lebt auf der Straße.\n\nMy piano teacher lives down the road.",
                "Seine Leiche fiel zu Boden.\n\nHis body fell to the ground.",
                "Es wird regnen Frühling.\n\nIt will rain all spring.",
                "Mein Sohn ist immer als Letzter.\n\nMy son is always chosen last.",
                "Das Mädchen war eine großartige Tänzerin.\n\nThe girl was a great dancer.",
                "Das Maler ist sehr geschickt.\n\nThat painter is very skillful.",
                "Meine Mutter verkauft Brot auf dem Markt.\n\n",
                "Die Vereinigten Staaten Militär ist zu groß.\n\nThe United States' military is too big.",
                "Haben Sie etwas dagegen, wenn ich meinen Freund mitbringen?\n\nDo you mind if I bring my friend?",
                "Wir essen mittags essen.\n\nWe will eat food around noon.",
                "Ich kann nicht wach bleiben bis nach Mitternacht.\n\nI can't stay awake past midnight.",
                "Montag bedeutet wieder an die Arbeit.\n\nMonday means back to work.",
                "Meine Mutter kocht immer mir Frühstück.\n\nMy mother always cooks me breakfast.",
                "Sein Nachbar immer verlässt seine Tür zu öffnen.\n\nHis neighbor always leaves his door open.",
                "Mein Neffe ist der Sohn meiner Schwester.\n\nMy nephew is my sister's son.",
                "Morgen ist ein neuer Tag.\n\nTomorrow is a  new day.",
                "Er sagt, er will nichts zu Weihnachten.\n\nHe says he wants nothing for christmas.",
                "Seine Position ist niedriger als meine.\n\nHis position is lower than mine.",
                "Er ist immer noch der beste Koch in der Stadt.\n\nHe is still the best chef in town.",
                "Dein Freund wohnt nördlich der Stadt.\n\nYour friend lives north of town.",
                "Meine Lieblingsnummer ist 12.\n\nMy favorite number is 12.",
                "Oft sitze ich unter dem Baum in den Parks.\n\nOften I sit under the tree in the park.",
                "Er isst, ohne anzuhalten!\n\nHe eats without stopping!",
                "Dein Onkel muss operiert auf seinem Bein.\n\nYour uncle needs surgery on his leg.",
                "Meine Ort ist bekannt für seine Meeresfrüchte bekannt.\n\nMy town is known for it's seafood.",
                "Wir müssen genau nach Osten fahren.\n\nWe need to head due east.",
                "Ich bin zu meiner Oma gehen für Ostern.\n\nI am going to my grandma's for Easter.",
                "Meine Vorfahren wurden von der Pest getötet.\n\nMy ancestors were killed by the plague.",
                "Mein bester Freund ist ein Pflegekind.\n\nMy best friend is a foster child.",
                "Kennen Sie den Platz Kaffee Innenstadt?\n\nDo you know the coffee place downtown?",
                "Unser Priester ist unser Fußball-Torwart.\n\nOur priest is our soccer goalie.",
                "Der Prinz wird in Kürze mit der Krone.\n\nThe prince will soon be wearing the crown.",
                "Meine Stromrechnung ist teuer.\n\nMy electricity bill is expensive.",
                "Seine Frau ist ein Anwalt.\n\nHis wife is a lawyer.",
                "Die allgemeine geschützt sein Reich.\n\nThe general protected his empire.",
                "Sie wurde eine Witwe nach dem Krieg.\n\nShe became a widow after the war.",
                "Sein Vater ist ein Schiffsbauer Rentner.\n\nHis father is a retired shipbuilder.",
                "Er wurde für Analysis und Geometrie registriert.\n\nHe was registered for calculus and geometry.",
                "Der Schäfer seine Schafe verloren.\n\nThe shepherd lost his sheep.",
                "Er war Mitarbeiter an der Autofabrik.\n\nHe was an employee at the car factory.",
                "Ich schickte meine behaarte Bruder meinem Friseur.\n\nI sent my hairy brother to my barber.",
                "Es gibt nicht viele Schiffbauer mehr.\n\nThere are not many shipbuilders anymore.",
                "der Schiffer spotted Land.\n\nthe sailor spotted land.",
                "Ich möchte nicht in die Schule gehen heute.\n\nI dont want to go to school today.",
                "Sein Freund Lieblingsfarbe war immer schwarz.\n\nHis friend's favorite color was always black.",
                "Meine Schwester fiel aus einem Baum heute.\n\nMy sister fell out of a tree today.",
                "Er brachte seinen sechs Katzen.\n\nHe brought along his six cats.",
                "Ich fing einen großen Fisch in dem kleinen See.\n\nI caught a huge fish in that small lake.",
                "Die Legende sagt, dass es Seelen in diesem Wald.\n\nThe legend says there are souls in that forest.",
                "Er riss die Seite direkt aus dem Buch.\n\nHe ripped the page right out of the book.",
                "Sollten ich zum Schluss meines Romans?\n\nShould I finish my novel?",
                "Mein Staat Unabhängigkeit im Jahr 2000.\n\nMy state gained independence in 2000.",
                "Welche Stadt ist dein Freund aus?\n\nWhich city is your friend from?",
                "Ich stellte den Truthahn auf den Tisch.\n\nI placed the turkey on the table.",
                "Ich bin zu Fuß bis Stuart Straße.\n\nI am walking up Stuart street.",
                "Beobachten Sie die Vögel fliegen nach Süden für den Winter.\n\nWatch the birds fly south for the winter.",
                "Meine Tante macht den besten Kuchen der Welt.\n\nMy aunt makes the best pie in the world.",
                "Wir halten eine Party für unsere Großeltern.\n\nWe are holding a party for our grandparents.",
                "Das Auto kostet zwanzigtausend Dollar.\n\nThe car costs twenty thousand dollars.",
                "Er unterrichtete seine Tochterwie man Auto fährt.\n\nHe taught his daughter how to drive.",
                "Wir müssen überleben den Sturm.\n\nWe must survive the storm.",
                "Er wurde unter dem Dock versteckt.\n\nHe was hiding under the dock.",
                "Der Mann trug uneheliche Geld.\n\nThe man was carrying illegitimate money.",
                "Sie müssen eine Unterschrift auf dem Scheck setzen.\n\nYou must put a signature on your check.",
                "Sie in der Vergangenheit datiert.\n\nThey dated in the past.",
                "Sein Vermieter verlangte seine Miete.\n\nHis landlord was demanding his rent.",
                "Er hat eine Verlobte für zwei Jahre hatten.\n\nHe has had a fiancee for two years.",
                "Durchführen sie ihr Lieblingslied auf der Violine.\n\nShe performed her favorite song on violin.",
                "Meine Verwandten gerne Alkohol im Urlaub trinken.\n\nMy relatives love to drink alcohol on holidays.",
                "Vielleicht würde er besser aussehen, wenn er rasiert.\n\nPerhaps he would look better if he shaved.",
                "Vor Wüste müssen wir zu Abend essen.\n\nBefore desert we must eat dinner.",
                "Er sang immer noch das vorherige Lied.\n\nHe was still singing the previous song.",
                "Es war kein Verkehr in den Vororten.\n\nThere was no traffic in the suburbs.",
                "Es gab viele Waisen auf der Suche nach einem neuen Zuhause.\n\nThere were many orphans looking for a new home.",
                "Das Haus wurde wegen jedermanns harte Arbeit gebaut.\n\nThe house was built because of everyone's hard work.",
                "Die ganze Welt drehte sich nach der Schnee weiß.\n\nThe whole world turned white after the snow.",
                "Welches Auto würdest Du am liebsten fahren?\n\nWhich car would you prefer to drive?",
                "Ich wurde von dem Wirt begrüßt.\n\nI was greeted by the innkeeper.",
                "Sie musste zwei Wochen warten, um das Konzert zu sehen.\n\nShe needed to wait two weeks to see the concert.",
                "Er wohnt auf der schlechten Seite der Stadt.\n\nHe lives on the bad side of town.",
                "Er will sich in einer Limousine abgeholt.\n\nHe wants to be picked up in a limo.",
                "Schauen Sie dieses Wort in einem Wörterbuch.\n\nLook this word up in a dictionary.",
                "Der Student war nicht würdig, die Lounge geben.\n\nThe student was not worthy to enter the lounge.",
                "Er zählte den Preis an seinen Fingern.\n\nHe counted the price on his fingers.",
                "Die meisten von uns haben zehn Finger und zehn Zehen.\n\nMost of us have ten fingers and ten toes.",
                "Wann ist die Party heute Abend?\n\nWhat time is the party tonight?",
                "Ich erlebte eine Tragödie gestern.\n\nI witnessed a tragedy yesterday.",
                "Sein Vater war Zimmermann für die Stadt.\n\nHis father was a carpenter for the town.",
                "Die Kinder gedacht, die Zukunft.\n\nThe children imagined the future.",
                "Wir müssen zusammenarbeiten.\n\nWe will need to work together.",
                "Es waren zwanzig verschiedene Biersorten vom Fass.\n\nThere were twenty different beers on tap.",
                "Der Hund wurde zwischen zwei Zäunen stecken.\n\nThe dog was stuck between two fences."
            });
        }

        //updates the main page word & definition
        private void GetWordOfTheDay()
        {
            using (Mutex mutex = new Mutex(true, "MyMutex"))
            {
                mutex.WaitOne();
                try
                {
                    lastUpdate = (DateTime)IsolatedStorageSettings.ApplicationSettings["lastUpdate"];
                    count = (int)IsolatedStorageSettings.ApplicationSettings["updateCount"];
                    dailyChange = (bool)IsolatedStorageSettings.ApplicationSettings["trueIfDaily"];

                    //UPDATE DAILY
                    if (DateTime.Now.Day != lastUpdate.Day && dailyChange)
                    {
                        chosenIndices = (List<int>)IsolatedStorageSettings.ApplicationSettings["chosenIndices"];
                        chosenIndices.Add(count);

                        if (chosenIndices.Count() == wordList.Count())
                        {//resets index list if every word has been chosen || will add more words with update!
                            chosenIndices = new List<int>();
                        }

                        count = rand.Next(wordList.Count);

                        while(chosenIndices.Contains<int>(count)){
                            count = rand.Next(wordList.Count);
                        }//returns a new count unused before

                        IsolatedStorageSettings.ApplicationSettings.Remove("chosenIndices");
                        IsolatedStorageSettings.ApplicationSettings.Add("chosenIndices", chosenIndices);

                        IsolatedStorageSettings.ApplicationSettings.Remove("updateCount");
                        IsolatedStorageSettings.ApplicationSettings.Add("updateCount", count);

                        IsolatedStorageSettings.ApplicationSettings.Remove("lastUpdate");
                        IsolatedStorageSettings.ApplicationSettings.Add("lastUpdate", DateTime.Now);
                        IsolatedStorageSettings.ApplicationSettings.Save();
                    }
                    //UPDATE HOURLY
                    else if (DateTime.Now.Hour != lastUpdate.Hour && !dailyChange)
                    {
                        chosenIndices = (List<int>)IsolatedStorageSettings.ApplicationSettings["chosenIndices"];
                        chosenIndices.Add(count);

                        if (chosenIndices.Count() == wordList.Count())
                        {//resets index list if every word has been chosen || will add more words with update!
                            chosenIndices = new List<int>();
                        }


                        count = rand.Next(wordList.Count);

                        while(chosenIndices.Contains<int>(count)){
                            count = rand.Next(wordList.Count);
                        }//returns a new count unused before

                        IsolatedStorageSettings.ApplicationSettings.Remove("chosenIndices");
                        IsolatedStorageSettings.ApplicationSettings.Add("chosenIndices", chosenIndices);

                        IsolatedStorageSettings.ApplicationSettings.Remove("updateCount");
                        IsolatedStorageSettings.ApplicationSettings.Add("updateCount", count);

                        IsolatedStorageSettings.ApplicationSettings.Remove("lastUpdate");
                        IsolatedStorageSettings.ApplicationSettings.Add("lastUpdate", DateTime.Now);
                        IsolatedStorageSettings.ApplicationSettings.Save();

                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }

            wordOfDayText.Text = wordList[count];
            definitionText.Text = "- "+definitionList[count];
            useItInSentenceText.Text = sentenceList[count];
            updateText.Text = String.Format("Last Update: {0:ddd} {0:t}",lastUpdate);

            string[] split = useItInSentenceText.Text.Split(delimiter, StringSplitOptions.None);
            englishSentence = split[1];
            germanSentence = split[0];

            //this is good for testing the tile, but the app never opens
            //UpdateTile();
        }

        //boolean to tell if the app has been opened before
        //so i don't have to save to isolated storage every time the app is opened
        private static bool hasSeenIntro;
        /// <summary>Will return false only the first time a user ever runs this.
        /// Everytime thereafter, a placeholder file will have been written to disk
        /// and will trigger a value of true.</summary>
        public static bool HasUserSeenIntro()
        {
            if (hasSeenIntro) return true;

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists("EmptyIfFirstTime"))
                {
                    // just write a placeholder file one byte long so we know they've landed before
                    using (var stream = store.OpenFile("EmptyIfFirstTime", FileMode.Create))
                    {
                        stream.Write(new byte[] { 1 }, 0, 1);
                    }
                    return false;
                }

                hasSeenIntro = true;
                return true;
            }
        }

        //ONLY fires the first time the app is loaded to store data for scheduled agent
        public void SaveTileInfo()
        {
            //counts the updates to the live tile
            count = 0;

            using (Mutex mutex = new Mutex(true, "MyMutex"))
            {
                mutex.WaitOne();
                try
                {
                    chosenIndices = new List<int>();
                    IsolatedStorageSettings.ApplicationSettings["wordList"] = wordList;
                    IsolatedStorageSettings.ApplicationSettings["definitionList"] = definitionList;
                    IsolatedStorageSettings.ApplicationSettings["sentenceList"] = sentenceList;
                    IsolatedStorageSettings.ApplicationSettings["chosenIndices"] = chosenIndices;
                    IsolatedStorageSettings.ApplicationSettings["updateCount"] = count;
                    IsolatedStorageSettings.ApplicationSettings["lastUpdate"] = DateTime.Now;
                    IsolatedStorageSettings.ApplicationSettings["trueIfDaily"] = true;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        private void AddAgent(string name)
        {
            periodicTask = ScheduledActionService.Find(name) as PeriodicTask;
            if (periodicTask != null)
            {
                RemoveAgent(name);
            }
 
            periodicTask = new PeriodicTask(name);
            periodicTask.Description = "LiveTileHelperUpdateTask";
           
            try
            {
                ScheduledActionService.Add(periodicTask);
 
                // If debugging is enabled, use LaunchForTest to launch the agent in 5 seconds.
                #if(DEBUG_AGENT)
                ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(10));
                #endif
 
            }
            catch (InvalidOperationException)
            {
                //ScheduledAgentCheckBox.IsChecked = false;
            }
            catch (SchedulerServiceException)
            {
                //ScheduledAgentCheckBox.IsChecked = false;
            }
        }

        private void RemoveAgent(string name)	 	
	        {	 	
	            try	 	
	            {	 	
	                ScheduledActionService.Remove(name);	  
	            }	 	
	            catch (Exception)	 	
	            {	 	
	            }	 	
	        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            using (Mutex mutex = new Mutex(true, "MyMutex"))
            {
                mutex.WaitOne();
                try
                {
                    count = (int)IsolatedStorageSettings.ApplicationSettings["updateCount"];
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }

            UpdateTile();
        }

        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ApplicationBarIconButton_Click_3(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.RelativeOrAbsolute));
        }
        
        private void UpdateTile()
        {
            if (isTileUpdateInProgress)
            {
                return;
            }
            isTileUpdateInProgress = true;

            using (Mutex mutex = new Mutex(true, "MyMutex"))
            {
                mutex.WaitOne();
                try
                {
                    var frontGrid = new Grid();
                    frontGrid.Width = 350;
                    frontGrid.Height = 350;
                    //int selectedIndex = rand.Next(loadWordList.Count);

                    TextBlock wordsBlock = new TextBlock()
                    {
                        Text = wordList[count],
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        FontSize = 44,
                        Margin = new Thickness(8, 5, 5, 5)
                    };

                    TextBlock defsBlock = new TextBlock()
                    {
                        Text = "\n- " + definitionList[count],
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        FontSize = 36,
                        Margin = new Thickness(22, 25, 12, 5)
                    };
                    TextBlock lineBlock = new TextBlock()
                    {
                        Text = "___________________________________",
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        FontSize = 60,
                        Margin = new Thickness(0, 260, 0, 0)
                    };
                    frontGrid.Children.Add(wordsBlock);
                    frontGrid.Children.Add(defsBlock);
                    frontGrid.Children.Add(lineBlock);


                    //////////////////////////
                    //grid to hold BACK tile data
                    var backGrid = new Grid();
                    backGrid.Width = 350;
                    backGrid.Height = 350;
                    //int selectedIndex = rand.Next(loadWordList.Count);

                    TextBlock sentencesBlock = new TextBlock()
                    {
                        Text = sentenceList[count],
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        FontSize = 34,
                        Margin = new Thickness(5, 5, 15, 5)
                    };
                    TextBlock line3Block = new TextBlock()
                    {
                        Text = "___________________________________",
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        FontSize = 60,
                        Margin = new Thickness(0, 260, 0, 0)
                    };
                    backGrid.Children.Add(sentencesBlock);
                    backGrid.Children.Add(line3Block);


                    //////////////////////
                    //initialize radtilehelper extendedData

                    RadFlipTileData flipData = new RadFlipTileData();

                    flipData.Title = "German Word a Day";
                    flipData.VisualElement = frontGrid;
                    flipData.BackVisualElement = backGrid;
                    flipData.IsTransparencySupported = true;


                    ///////////////////////////////////
                    //delete old tile, add new one
                    ShellTile tile = LiveTileHelper.GetTile(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                    if (tile != null)
                    {
                        tile.Delete();
                    }

                    LiveTileHelper.CreateTile(flipData, new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute), false);

                    AddAgent(periodicTaskName);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            Dispatcher.BeginInvoke(() =>
            {
                isTileUpdateInProgress = false;
            });

        }

        //SPEAK ENGLISH
        private bool isSpeakInProgressEN = false;
        private async void SpeakEnglish(object sender, RoutedEventArgs e)
        {
            if (isSpeakInProgressEN)
            {
                return;
            }
            isSpeakInProgressEN = true;

            var englishVoice = InstalledVoices.All
                .Where(voice => voice.Language.Contains("en") & voice.Gender == VoiceGender.Female)
                .FirstOrDefault();
            if (englishVoice == null)
            {
                MessageBox.Show(
               "To use this feature, please activate the English Language by going to:\nSettings > Speech > Speech Language > English",
               "Sorry, you do not have English Text-to-Speech",
               MessageBoxButton.OK);
            }
            else
            {
                _speakerEN.SetVoice(englishVoice);
                await _speakerEN.SpeakTextAsync(englishSentence);
            }

            Dispatcher.BeginInvoke(() =>
            {
                isSpeakInProgressEN = false;
            });
        }


        //SPEAK GERMAN
        private bool isSpeakInProgressDE = false;
        private async void SpeakGerman(object sender, RoutedEventArgs e)
        {
            if (isSpeakInProgressDE)
            {
                return;
            }
            isSpeakInProgressDE = true;
            
            var germanVoice = InstalledVoices.All
                .Where(voice => voice.Language.Equals("de-DE") & voice.Gender == VoiceGender.Male)
                .FirstOrDefault();
            if (germanVoice == null)
            {
                MessageBox.Show(
               "To use this feature, please activate the German Language by going to:\nSettings > Speech > Speech Language > Deutsch",
               "Sorry, you do not have German Text-to-Speech",
               MessageBoxButton.OK);
            }
            else
            {
                _speakerDE.SetVoice(germanVoice);
                await _speakerDE.SpeakTextAsync(germanSentence);
            }

            Dispatcher.BeginInvoke(() =>
            {
                isSpeakInProgressDE = false;
            });
        }
    }
}
