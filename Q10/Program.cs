using System;
using System.Data.Entity;
using System.Linq;

namespace Q10
{
    public class Program
    {
        protected static DBforDevMA2Model Context { get; set; } = new DBforDevMA2Model();

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Veterinary Clinic Internal System!");
            Console.WriteLine("\nPlease select");
            Console.WriteLine("\t1 to delete all consultations on a future day");
            Console.WriteLine("\t2 to see the revenue for each species");
            Console.WriteLine("\tQ to exit");

            var ch = ' ';
            while (!"12q".Contains(ch))
            {
                ch = Char.ToLower(Console.ReadKey().KeyChar);
                Console.WriteLine("\b\b");
            }
            
            if (ch == '1') DeleteConsultations();
            else if (ch == '2') SpeciesRevenue();
            else Environment.Exit(0);
        }

        private static void SpeciesRevenue()
        {
            Console.Write("\nPlease enter a month in the format 'M-yyyy': ");

            var date = new DateTime();
            var isValidDate = false;

            while (!isValidDate)
            {
                isValidDate = DateTime.TryParse("1-" + Console.ReadLine(), out date);

                if (!isValidDate)
                {
                    Console.WriteLine("The entered month was not registered as a valid month.");
                    Console.Write("\nPlease enter a valid month in the format 'M-yyyy': ");
                }
            }

            var dateStr = date.ToString("MMMM yyyy");
            dateStr = dateStr.Substring(0, 1).ToUpper() + dateStr.Substring(1);
            Console.WriteLine($"\nTreatments in {dateStr}:");

            var treatmentsInMonth = 
                from t in Context.treatments
                from s in Context.species
                join p in Context.pets on s.specieid equals p.speciesid
                join c in Context.consultations on p.id equals c.pet
                where c.starttime.Year == date.Year 
                && c.starttime.Month == date.Month
                && t.consultations.Contains(c)
                orderby c.starttime
                select new
                {
                    c.starttime,
                    petname = p.name,
                    species = s.name,
                    treatment = t.treatmenttext,
                    t.price
                };

            var speciesRevenue =
                from tim in treatmentsInMonth
                group tim by tim.species into sp
                select new { species = sp.Key, revenue = sp.Sum(x => x.price) };

            var speciesRevenueWithEmpty =
                from s in Context.species
                from sr in speciesRevenue.Where(sr => sr.species == s.name).DefaultIfEmpty()
                select new { species = s.name, sr.revenue };

            Console.WriteLine($"\n{"Start time", -25}{"Pet name", -15}{"Species", -10}{"Treatment", -25}{"Price", -5}");
            foreach (var t in treatmentsInMonth)
            {
                Console.WriteLine($"{t.starttime, -25}{t.petname, -15}{t.species, -10}{t.treatment, -25}{t.price, -5}");
            }

            Console.WriteLine($"\n\nRevenue in {dateStr}:");
            Console.WriteLine($"\n{"Species", -10}{"Revenue", 10}");
            foreach (var sr in speciesRevenueWithEmpty)
            {
                Console.WriteLine($"{sr.species, -10}{sr.revenue ?? 0, 10}");
            }
            Console.WriteLine($"\n{"Total:", -10}{speciesRevenue.Sum(sr => sr.revenue) ?? 0, 10}");

            Console.Write("\n\nPress any key to exit ...");
            Console.ReadKey();
        }

        private static void DeleteConsultations()
        {
            Console.Write("\nPlease enter a future date for deletion in the format 'd-M-yyyy': ");

            var date = new DateTime();
            var isValidDate = false;

            while (!isValidDate)
            {
                isValidDate = DateTime.TryParse(Console.ReadLine(), out date);

                if (!isValidDate)
                {
                    Console.WriteLine("The entered date was not recognized as a valid date.");
                    Console.Write("\nPlease enter the desired date in the format 'd-M-yyyy': ");
                }
                else if (date <= DateTime.Now)
                {
                    Console.WriteLine("You must enter a valid day in the future.");
                    Console.Write("\nPlease enter the desired date in the format 'd-M-yyyy': ");
                    isValidDate = false;
                }
            }

            var consultationsToDelete = Context.consultations
                .Where(c => DbFunctions.TruncateTime(c.starttime) == date.Date)
                .Select(c => c)
                .OrderBy(c => c.starttime);

            if (consultationsToDelete.Count() == 0)
            {
                Console.WriteLine($"\nThere are no consultations scheduled for {date.ToShortDateString()}. ");
            }
            else
            {
                Console.WriteLine($"\nRegistered consultations on {date.ToShortDateString()}:");
                Console.WriteLine($"\n{"Start time",-15}{"Pet owner", -15}{"Address", -25}{"Pet name", -15}");

                foreach (var c in consultationsToDelete)
                {
                    var owner = c.pet1.petowner1;
                    Console.WriteLine($"{c.starttime.TimeOfDay,-15}{owner.name, -15}{owner.address, -25}{c.pet1.name, -15}");
                }

                Console.Write("\nAre you sure you want to delete the consultations (y/n)? ");

                var key = ' ';
                while (!"yn".Contains(key))
                {
                    key = Char.ToLower(Console.ReadKey().KeyChar);
                    Console.WriteLine("\b\b");
                }

                if (key == 'y')
                {
                    Context.consultations.RemoveRange(consultationsToDelete);
                    Context.SaveChanges();

                    Console.WriteLine("\nThe consultations were deleted. ");
                }
                else
                {
                    Console.WriteLine("\nDeletion was cancelled. ");
                }
            }

            Console.Write("\nPress any key to exit ...");
            Console.ReadKey();
        }
    }
}
