using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone_ContactTracingDemo
{// Not for production
    //
    //
    static class ProgramParrameters
    {
        public static int ContactTracingLength;
    }
    public class Contact
    {
        public enum ContactType {Entrance, Exit};
        public Contact ( string entrantID, string establishmentID, DateTime timeOfContact, ContactType contactType)
        {
            EntrantID = entrantID;
            EstablishmentID = establishmentID;
            TimeOfContact = timeOfContact;
            ThisContactType = contactType;
        }
        public bool SetContactPair(Contact contact)
        {
            bool isCompatible = true;
            //Checks to make sure it has the correct pair
            if(contact.contactPair.EstablishmentID != EstablishmentID)
            {
                isCompatible = false;
            }
            if(contact.contactPair.ThisContactType == ThisContactType)
            {
                isCompatible = false;
            }
            if(contact.EntrantID != EntrantID)
            {
                isCompatible = false;
            }

            if(isCompatible)
            {
                contactPair = contact;
            }
            return isCompatible;
        }
        public Contact contactPair { get; private set; }
        public string EntrantID { get; private set; }
        public string EstablishmentID { get; private set; }
        public DateTime TimeOfContact { get; private set; }
        public ContactType ThisContactType { get; private set; }
    }

    public class Establishment
    {
        public string ID { get; private set; }
        public Queue<Contact>[,,] Contacts { get; private set; }//[int day, int month, int year]\
        public List<string> Notifications { get; private set; }

        public Establishment(string iD)
        {
            Notifications = new List<string>();
            Contacts = new Queue<Contact>[40, 13, 100];

            //Populates Contacts with lists

            for(int day = 0; day < 40; day++)
            {
                for(int month = 0; month < 13; month++)
                {
                    for(int year = 0; year < 100; year++)
                    {
                        Contacts[day, month, year] = new Queue<Contact>();
                    }
                }
            }

            ID = iD;
        }
        public void AddContact(Entrant entrant, DateTime TimeOfContact, Contact.ContactType contactType)
        {
            Contact CurrentContact = new Contact(entrant.ID, ID, TimeOfContact, contactType);
            Contacts[TimeOfContact.Day, TimeOfContact.Month, TimeOfContact.Year - 2000].Enqueue(CurrentContact);
        }
        public List<Contact> ContactTraceEstablishment(DateTime contactTimeStart, DateTime contactTimeEnd)
        {
            Queue<Contact> ContactOnDate = new Queue<Contact>(Contacts[contactTimeStart.Day, contactTimeStart.Month, contactTimeStart.Year]);

            bool isDone = false;
            List<Contact> TracedContacts = new List<Contact>();

            while(!isDone)
            {
                Contact CurContact = ContactOnDate.Dequeue();
                if(contactTimeEnd < CurContact.TimeOfContact)
                {//Current contact is later than trace parameters
                    isDone = true; 
                }
                else if(contactTimeStart < CurContact.TimeOfContact)
                {//currect contact is within trace parameter
                    TracedContacts.Add(CurContact);
                }
            }

            Notifications.Add("Covid Exposure on: " + 
                contactTimeStart.Day + "/" + contactTimeStart.Month + "/" + contactTimeStart.Year +
                " between " + contactTimeStart.Hour + " and " + contactTimeEnd.Hour);
            return TracedContacts;
            
            
        }

    }
    public class Entrant
    {
        Contact PreviousContact;
        public Entrant(string id)
        {
            Contacts = new Queue<Contact>();
            Notifications = new List<string>();
            ID = id;
        }
        public void NotifyEntrantOfContact(DateTime DateOfContact, string ContactEstablishment)
        {
            Notifications.Add("Possible contact with a covid possitive patient on " + DateOfContact.Day + "/" + DateOfContact.Month + "/" + " at " + ContactEstablishment);
        }
        public void NotifyEntrantOfPositiveTestResult(DateTime DateOfContact)
        {
            Notifications.Add("You have recieved a positive test result on " + DateOfContact.Day + "/" + DateOfContact.Month + "/" + ". Please seek medical care immediately ");
        }
        public string ID { get; private set; }
        public Queue<Contact> Contacts { get; private set; }
        public List<string> Notifications { get; private set; }
        public void AddContact(Establishment establishment, DateTime TimeOfContact, Contact.ContactType contactType)
        {
            Contact CurrentContact = new Contact(ID, establishment.ID, TimeOfContact, contactType);
            if(CurrentContact.ThisContactType == Contact.ContactType.Exit)
            {
                PreviousContact.SetContactPair(CurrentContact);
            }
            else
            {
                PreviousContact = CurrentContact;
            }
            Contacts.Enqueue(CurrentContact);
        }
        public List<Contact> ContactTraceEntrant(DateTime CurrentDate)
        {
            bool isDoneUpdatingContacts = false;
            while (isDoneUpdatingContacts)
            {
                if (Contacts.Peek().TimeOfContact > (CurrentDate - new TimeSpan(ProgramParrameters.ContactTracingLength, 0, 0, 0, 0)))
                {
                    Contacts.Dequeue();
                }
                else
                {
                    isDoneUpdatingContacts = true;
                }
            }

            return new List<Contact>(Contacts.ToList());
        }
    }
    public interface iDatabase
    {
        List<Entrant> GetListOfEntrants();
        List<Establishment> GetListOfEstablishments();
        bool RegisterEntrant(string ID);
        bool RegisterEstablishment(string ID);
        Entrant GetEntrant(string EntrantID);
        Establishment GetEstablishment(string EstablishmentID);

    }
    class Demo_Database:iDatabase
    {//This database object is only for demostration, testing and simulation only. 
        public Dictionary<string, Entrant> EntrantDictionary;
        public Dictionary<string, Establishment> EstablishmentDictionary;
        public Demo_Database()
        {
            EntrantDictionary = new Dictionary<string, Entrant>();
            EstablishmentDictionary = new Dictionary<string, Establishment>();
        }
        public List<Entrant> GetListOfEntrants()
        {
            return new List<Entrant>(EntrantDictionary.Values);
        }
        public List<Establishment> GetListOfEstablishments()
        {
            return new List<Establishment>(EstablishmentDictionary.Values);
        }
        public Entrant GetEntrant(string EntrantID)
        {
            if(EntrantDictionary.ContainsKey(EntrantID))
            {
                return EntrantDictionary[EntrantID];
            }
            else
            {
                return null;
            }
            
        }
        public Establishment GetEstablishment(string EstablishmentID)
        {
            if (EstablishmentDictionary.ContainsKey(EstablishmentID))
            {
                return EstablishmentDictionary[EstablishmentID];
            }
            else
            {
                return null;
            }
        }
        public bool RegisterEntrant(string ID)
        {
            if(EntrantDictionary.ContainsKey(ID))
            {
                return false;
            }
            else
            {
                EntrantDictionary.Add(ID, new Entrant(ID));
                return true;
            }
        }
        public bool RegisterEstablishment(string ID)
        {
            if (EstablishmentDictionary.ContainsKey(ID))
            {
                return false;
            }
            else
            {
                EstablishmentDictionary.Add(ID, new Establishment(ID));
                return true;
            }
        }
    }

    abstract class DemoConsolePageTemplate
    {
        abstract public bool Input(string UserInput);
        abstract public void Display();
        abstract public void DisplayHelp();
    }
    class EntrantPage: DemoConsolePageTemplate
    {
        Entrant CurEntrant;
        public override bool Input(string UserInput)
        {
            Entrant entrant = Program.database.GetEntrant(UserInput);
            if(entrant != null)
            {
                CurEntrant = entrant;
                return true;
            }
            else
            {
                return false;
            }
        }
        public override void Display()
        {
            Console.WriteLine("----DISPLAYING ENTRANT PAGE----");
            Console.WriteLine("-------------------------------------");
            if (CurEntrant != null)
            {
                Console.WriteLine("Entrant ID: " + CurEntrant.ID);
                Console.WriteLine("---Notifications---");
                if(CurEntrant.Notifications.Count != 0)
                {
                    foreach(string Notification in CurEntrant.Notifications)
                    {
                        Console.WriteLine(Notification);
                    } 
                }
                else
                {
                    Console.WriteLine("No Notifications Available.");
                }
                Console.WriteLine("--Contacts--");
                if (CurEntrant.Contacts.Count != 0)
                {
                    foreach (Contact contact in CurEntrant.Contacts)
                    {
                        if(contact.ThisContactType == Contact.ContactType.Entrance)
                        {
                            Console.Write("Entered '");
                        }
                        else
                        {
                            Console.Write("Exited '");
                        }
                        Console.WriteLine("' at " + contact.TimeOfContact);
                    }
                }
                else
                {
                    Console.WriteLine("No Contacts Available.");
                }
            }
            else
            {
                Console.WriteLine("Please type entrant ID to retrieve information");
            }
        }
        public override void DisplayHelp()
        {
            Console.WriteLine("Please type entrant ID to retrieve information. You may type -entList to see the list of entrant IDs");
        }

    }
    class EstablishmentPage : DemoConsolePageTemplate
    {
        Establishment currentEstablishment;
        DateTime currentSearchDateTime;
        public override bool Input(string UserInput)
        {
            bool isDone = false;
            switch (UserInput)
            {
                case "DAY":
                    while (!isDone)
                    {
                        Console.WriteLine("Please enter a new value...");
                        string PromptUserInput = Console.ReadLine();
                        if (!PromptUserInput.Any(Char.IsLetter) && !PromptUserInput.Any(Char.IsWhiteSpace))
                        {//Invalid if contains letters or white space
                            try
                            {
                                int NewDay = Convert.ToInt32(PromptUserInput);
                                if (NewDay > 1 && NewDay <= 31)
                                {
                                    currentSearchDateTime = new DateTime(currentSearchDateTime.Year, currentSearchDateTime.Month, NewDay);
                                    isDone = true;
                                }
                            }
                            catch (Exception)
                            {

                            }

                        }
                        if (!isDone)
                        {
                            Console.WriteLine("Invalid input. Kindly enter a value between 1-31");
                        }
                    }
                    return true;
                case "MONTH":
                    while (!isDone)
                    {
                        Console.WriteLine("Please enter a new value...");
                        int newMonth = -1;
                        string PromptUserInput = Console.ReadLine();
                        switch (PromptUserInput)
                        {
                            case "JAN": newMonth = 1; isDone = true; break;
                            case "FEB": newMonth = 2; isDone = true; break;
                            case "MAR": newMonth = 3; isDone = true; break;
                            case "APR": newMonth = 4; isDone = true; break;
                            case "MAY": newMonth = 5; isDone = true; break;
                            case "JUN": newMonth = 6; isDone = true; break;
                            case "JUL": newMonth = 7; isDone = true; break;
                            case "AUG": newMonth = 8; isDone = true; break;
                            case "SEP": newMonth = 9; isDone = true; break;
                            case "OCT": newMonth = 10; isDone = true; break;
                            case "NOV": newMonth = 11; isDone = true; break;
                            case "DEC": newMonth = 12; isDone = true; break;

                            default:
                                if (!PromptUserInput.Any(Char.IsLetter) && !PromptUserInput.Any(Char.IsWhiteSpace))
                                {//Invalid if contains letters or white space
                                    try
                                    {
                                        if (newMonth > 1 && newMonth <= 12)
                                        {
                                            newMonth = Convert.ToInt32(PromptUserInput);
                                            isDone = true;
                                        }
                                    }
                                    catch (Exception)
                                    {

                                    }

                                }
                                break;   
                        }//Gets an int from user input
                        if (!isDone)
                        {
                            Console.WriteLine("Invalid input. Kindly enter a value between 1-12 or the three letter initials (ex. JAN, FEB, MAR)");
                        }
                        else
                        {
                            currentSearchDateTime = new DateTime(currentSearchDateTime.Year, newMonth, currentSearchDateTime.Day);
                        }
                    }
                    return true;
                case "YEAR":
                    while (!isDone)
                    {
                        Console.WriteLine("Please enter a new value...");
                        string PromptUserInput = Console.ReadLine();
                        if (!PromptUserInput.Any(Char.IsLetter) && !PromptUserInput.Any(Char.IsWhiteSpace))
                        {//Invalid if contains letters or white space
                            try
                            {
                                int NewDay = Convert.ToInt32(PromptUserInput);
                                if (NewDay > 1 && NewDay <= 3000)
                                {
                                    currentSearchDateTime = new DateTime(currentSearchDateTime.Year, currentSearchDateTime.Month, NewDay);
                                    isDone = true;
                                }
                            }
                            catch (Exception)
                            {

                            }

                        }
                        if (!isDone)
                        {
                            Console.WriteLine("Invalid input.");
                        }
                    }
                    return true;

                default:
                    Establishment establishment = Program.database.GetEstablishment(UserInput);
                    if (establishment != null)
                    {
                        currentEstablishment = establishment;
                        currentSearchDateTime = Program.SimulationDateTime;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    break;
            }

        }
        public override void Display()
        {
            Console.WriteLine("----DISPLAYING ESTABLISHMENT PAGE----");
            Console.WriteLine("-------------------------------------");
            if (currentEstablishment != null)
            {
                Console.WriteLine("Entrant ID: " + currentEstablishment.ID);
                Console.WriteLine("---Notifications---");
                if (currentEstablishment.Notifications.Count != 0)
                {
                    foreach (string Notification in currentEstablishment.Notifications)
                    {
                        Console.WriteLine(Notification);
                    }
                }
                else
                {
                    Console.WriteLine("No Notifications Available.");
                }
                Console.WriteLine("--Contacts--");
                Console.WriteLine("Retrieving Contacts On " + currentSearchDateTime.Day + "/" + currentSearchDateTime.Month + "/" + currentSearchDateTime.Year + " (day/month/year)");
                Queue<Contact> contacts = currentEstablishment.Contacts[currentSearchDateTime.Day, currentSearchDateTime.Month, currentSearchDateTime.Year - 2000];
                if (contacts.Count != 0)
                {
                    foreach (Contact contact in contacts)
                    {
                        if (contact.ThisContactType == Contact.ContactType.Entrance)
                        {
                            Console.Write("'" + contact.EntrantID + "' Entered");
                        }
                        else
                        {
                            Console.Write("'" + contact.EntrantID + "' Exited");
                        }
                        Console.WriteLine("at " + contact.TimeOfContact);
                    }
                }
                else
                {
                    Console.WriteLine("No Contacts Available.");
                }

                Console.WriteLine("----");
                Console.WriteLine("You may type 'DAY', 'MONTH' or 'YEAR' to modify search parameters");

            }
            else
            {
                Console.WriteLine("Please type contact ID to retrieve information");
            }
        }
        public override void DisplayHelp()
        {
            Console.WriteLine("Please type contact ID to retrieve information");
            Console.WriteLine("You may type 'DAY', 'MONTH' or 'YEAR' to modify search parameters");
        }
    }
    class RegistrationPage : DemoConsolePageTemplate
    {
        string PreviousMessage;
        public override bool Input(string UserInput)
        {
            Console.WriteLine("Type...");
            Console.WriteLine("1 -> Entrant");
            Console.WriteLine("2 -> Establishment");
            Console.WriteLine("3 -> Cancel");
            string Type = Console.ReadLine();
            switch (Type)
            {
                case "1":
                    if(Program.database.RegisterEntrant(UserInput))
                    {
                        PreviousMessage = "Succesfully added entrant " + UserInput + ".";
                        return true;
                    }
                    else
                    {
                        PreviousMessage = "Registration failed! " + UserInput + "already in database.";
                        return false;
                    }
                case "2":
                    if (Program.database.RegisterEstablishment(UserInput))
                    {
                        PreviousMessage = "Succesfully added establishment " + UserInput + ".";
                        return true;
                    }
                    else
                    {
                        PreviousMessage = "Registration failed! " + UserInput + "already in database.";
                        return false;
                    }
                case "3":
                    PreviousMessage = "Registration cancelled.";
                    return true;
                default:
                    Console.WriteLine("Invalid input!");
                    return Input(UserInput);

            }
        }
        public override void Display()
        {
            Console.WriteLine("----REGISTRATION PAGE----");
            Console.WriteLine("-------------------------------------");
            if(PreviousMessage != null)
            {
                Console.WriteLine(PreviousMessage);
                Console.WriteLine("---");
            }
            Console.WriteLine("To register a new user, enter new user ID");

        }
        public override void DisplayHelp()
        {
            Console.WriteLine("To register a new user, enter new user ID");
        }
    }
    class ContactTracePage : DemoConsolePageTemplate
    {
        public override bool Input(string UserInput)
        {
            Entrant entrant = Program.database.GetEntrant(UserInput);
            if (entrant != null)
            {
                Console.WriteLine("Are you sure you want to mark " + UserInput + " positive? (Y/N)");
                bool isDone = false;

                while(!isDone)
                {
                    string confirmation = Console.ReadLine();
                    if(confirmation == "Y" || confirmation == "y" || confirmation == "yes")
                    {
                        Program.NotifyEntrantsOfContactWithCovid(Program.database.GetEntrant(UserInput), Program.SimulationDateTime);
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        public override void Display()
        {
            Console.WriteLine("----CONTACT TRACE PAGE----");
            Console.WriteLine("--------------------------");
            Console.WriteLine("Please type in user ID to mark as possitive");
        }
        public override void DisplayHelp()
        {
            Console.WriteLine("You may type in user ID to mark as possitive");
        }
    }
    class ContactLoggingPage : DemoConsolePageTemplate
    {
        Establishment currentEstablishment;
        public override bool Input(string UserInput)
        {
            if(UserInput == "EST")
            {
                Console.WriteLine("Please enter the establishment ID...");
                Establishment establishment = Program.database.GetEstablishment( Console.ReadLine());

                if(establishment != null)
                {
                    currentEstablishment = establishment;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(currentEstablishment == null)
            {
                return false;
            }
            else
            {
                Entrant entrant = Program.database.GetEntrant(UserInput);
                if(entrant != null)
                {
                    Console.WriteLine("Type...");
                    Console.WriteLine("'i' -> for entry");
                    Console.WriteLine("'o' -> for exit");
                    Console.WriteLine("'x' -> for cancel");
                    switch(Console.ReadLine())
                    {
                        case "i":
                            currentEstablishment.AddContact(entrant, Program.SimulationDateTime, Contact.ContactType.Entrance);
                            entrant.AddContact(currentEstablishment, Program.SimulationDateTime, Contact.ContactType.Entrance);
                            return true;
                        case "o":
                            currentEstablishment.AddContact(entrant, Program.SimulationDateTime, Contact.ContactType.Exit);
                            entrant.AddContact(currentEstablishment, Program.SimulationDateTime, Contact.ContactType.Exit);
                            return true;
                        case "x": return true;
                        default: return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        public override void Display()
        {
            Console.WriteLine("----CONTACT LOGGING PAGE----");
            Console.WriteLine("----------------------------");
            if(currentEstablishment != null)
            {
                Console.WriteLine("Logging for: " + currentEstablishment);
                Console.WriteLine("Please enter the entrant ID...");
            }
            else
            {
                Console.WriteLine("No establishment loaded. Please type 'EST' to log for an establishment...");
            }
        }
        public override void DisplayHelp()
        {
            Console.WriteLine("-You may type 'EST' to open establishment change prompt, then type the establishment ID to load that establishment");
            Console.WriteLine("-To log contact, load an establishment, then type the entrant ID, then type 'i' for entrance and 'o' for exit.");
        
        }
    }
    class ListPage : DemoConsolePageTemplate
    {
        public enum Mode { EST, ENT};
        public Mode currentMode = Mode.EST;
        public override bool Input(string UserInput)
        {
            switch(UserInput)
            {
                case "EST": currentMode = Mode.EST; return true;
                case "ENT": currentMode = Mode.ENT; return true;
                default: return false;
            }
        }
        public override void Display()
        {
            Console.WriteLine("----LIST PAGE----");
            Console.WriteLine("-------------------------------------");
            if (currentMode == Mode.ENT)
            {
                Console.WriteLine("--Showing Entrants--");
                List <Entrant> entrants = Program.database.GetListOfEntrants();
                foreach(Entrant entrant in entrants)
                {
                    if(entrant.Notifications.Count > 0)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine(entrant.ID);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
            else
            {
                Console.WriteLine("--Showing Establishments--");
                List<Establishment> establishments = Program.database.GetListOfEstablishments();
                foreach ( Establishment establishment in establishments)
                {
                    if (establishment.Notifications.Count > 0)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine(establishment.ID);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }
        public override void DisplayHelp()
        {
            Console.WriteLine("You may type 'EST' to see the list of establishments or type 'ENT' to see the list of entrants");
        }
    }


    internal class Program
    {
        public static DateTime SimulationDateTime;//Only for demo, testing and simulation
        public static iDatabase database;
        static Dictionary<string, DemoConsolePageTemplate> Pages;
        static ListPage listPage;
        static public  List<Entrant> NotifyEntrantsOfContactWithCovid (Entrant OriginPatient, DateTime ResultDateTime)
        {
            List<Entrant> EntrantsContacted = new List<Entrant>();

            List<Contact> EstablishmentsContacted = OriginPatient.ContactTraceEntrant(ResultDateTime);

            foreach (Contact Contact in EstablishmentsContacted)
            {
                //Checks only entry type contacts to avoid duplication
                if (Contact.ThisContactType == Contact.ContactType.Entrance)
                {
                    List<Contact> ContactTracedEntrantsInEstablishment = new List<Contact>(database.GetEstablishment(Contact.EstablishmentID).ContactTraceEstablishment(Contact.TimeOfContact, Contact.contactPair.TimeOfContact));
                    foreach(Contact contactedEntrant in ContactTracedEntrantsInEstablishment)
                    {
                        EntrantsContacted.Add(database.GetEntrant(contactedEntrant.EntrantID));
                    }
                }
            }

            OriginPatient.NotifyEntrantOfPositiveTestResult(ResultDateTime);

            return EntrantsContacted;

        }

        //Pages for the console UI. Only for demo, testing and simulation


        static void Instantiate()
        {
            //Instantiates for demo, testing and simulation
            SimulationDateTime = new DateTime(DateTime.Now.Ticks);
            database = new Demo_Database();

            //creates a simulation database
            {
                string[] names = {"a", "b", "c", "d", "e", "f", "g", "h", "i",
                "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u",
                "v", "w", "x", "y", "z" };

                foreach (string name in names)
                {
                    database.RegisterEstablishment(name);
                }

                foreach (string leadname in names)
                {
                    foreach (string trailname in names)
                    {
                        database.RegisterEntrant(leadname + trailname);
                    }
                }
            }

            //Instantiates DemoPages
            {
                Pages = new Dictionary<string, DemoConsolePageTemplate>();
                Pages.Add("EntrantPage", new EntrantPage());
                Pages.Add("EstablishmentPage", new EstablishmentPage());
                Pages.Add("ContactTracePage", new ContactTracePage());
                Pages.Add("ContactLoggingPage", new ContactLoggingPage());
                Pages.Add("RegistrationPage", new RegistrationPage());
                
                listPage = new ListPage();


            }


        }
        static void HandleUserInput(ref DemoConsolePageTemplate CurrentPage, ref Dictionary<string, DemoConsolePageTemplate> Pages)
        {
            //Input
            string UserInput = Console.ReadLine();
            if (UserInput.StartsWith("-"))
            {
                switch (UserInput)
                {//reads the inputs and outputs accordingly 
                    case "-": //returns to home page
                        CurrentPage = null;
                        break;
                    case "-est": //Gets a specific Establishment
                        CurrentPage = Pages["EstablishmentPage"];
                        break;
                    case "-ent": //Gets a specific Entrant
                        CurrentPage = Pages["EntrantPage"];
                        break;
                    case "-reg": //Opens the page for registering 
                        CurrentPage = Pages["RegistrationPage"];
                        break;
                    case "-con": //Opens the page for contact tracing 
                        CurrentPage = Pages["ContactTracePage"];
                        break;
                    case "-log": //Opens the page for contact tracing 
                        CurrentPage = Pages["ContactLoggingPage"];
                        break;
                    case "-estList": //Opens a list of all establishments and highlights contacts
                        listPage.currentMode = ListPage.Mode.EST;
                        CurrentPage = listPage;   
                        break;
                    case "-entList": //Opens a list of all entrants and highlights contacts
                        listPage.currentMode = ListPage.Mode.ENT;
                        CurrentPage = listPage;
                        break;
                    case "-help": //Prints the manual for the console UI

                        break;
                    case "--help": //Prints the manual for the page
                        if (CurrentPage != null)
                        {
                            CurrentPage.DisplayHelp();
                        }
                        else
                        {
                            Console.WriteLine("No Page Loaded. To see the list of commands, please type '-help'");
                            HandleUserInput(ref CurrentPage, ref Pages);
                        }
                        break;
                    default:
                        Console.WriteLine("Command not recognized. To see the list of commands, please type '-help'");
                        HandleUserInput(ref CurrentPage, ref Pages);
                        break;

                }
            }
            else if (CurrentPage != null)
            {
                if (CurrentPage.Input(UserInput))
                {
                }
                else
                {
                    Console.WriteLine("Command not recognized. To see the list of commands, please type '--help'");
                    HandleUserInput(ref CurrentPage, ref Pages);
                }
            }
        }
        static void Main(string[] args)
        {//This code is used to demonstrate, simulate and test the contact tracing ability of the application
            Instantiate();

            DemoConsolePageTemplate CurrentPage = null;
            while(true)
            {//main loop
                //Display
                Console.Clear();
                if(CurrentPage != null)
                {
                    CurrentPage.Display();
                }
                else
                {
                    Console.WriteLine("Welcome! To see the list of commands, type in '-help' ");
                }

                HandleUserInput(ref CurrentPage, ref Pages);



            }
        }
    }
}
