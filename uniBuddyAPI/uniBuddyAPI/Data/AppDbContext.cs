//using Microsoft.EntityFrameworkCore;
//using System.Collections.Generic;
//using System.Reflection.Emit;
//using uniBuddyAPI.Models;

//namespace uniBuddyAPI.Data
//{
//    public class AppDbContext : DbContext
//    {

        //this is not being used at all, will delete later if not needed at all
        //doubt it will be needed because we using firebase



//        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

//        public DbSet<User> Users { get; set; } 
//        public DbSet<Timetable> Timetable { get; set; }
//        public DbSet<Goal> Goals { get; set; }
//        public DbSet<GoalSelection> GoalSelections { get; set; }

//        protected override void OnModelCreating(ModelBuilder b)
//        {
//            b.Entity<User>().HasIndex(u => u.Email).IsUnique();
//            b.Entity<User>().Ignore(u => u.Password);

//            //set timetable for the class
//            b.Entity<Timetable>().HasData(
//                new Timetable { Id = 1, TimeTableId = "t1", Module = "PROG1212", DayOfWeek = "Mon", StartTime = "08:00", EndTime = "10:00", Venue = "CR9", Lecturer = "Miss van Zyl" },
//                new Timetable { Id = 2, TimeTableId = "t2", Module = "DBAS1111", DayOfWeek = "Tue", StartTime = "11:00", EndTime = "12:00", Venue = "CR5", Lecturer = "Mr Bartie" },
//                new Timetable { Id = 3, TimeTableId = "t3", Module = "INSY7121", DayOfWeek = "Thu", StartTime = "09:00", EndTime = "11:00", Venue = "LR22", Lecturer = "Miss Bekker" }
//            );

//            //the 6 predifned goals for the app
//            b.Entity<Goal>().HasData(
//                new Goal { Id = 1, GoalId = "g1", Title = "Take 10 Notes", Points = 50 },
//                new Goal { Id = 2, GoalId = "g2", Title = "Study 1 Hour", Points = 40 },
//                new Goal { Id = 3, GoalId = "g3", Title = "Submit Assignment Early", Points = 30 },
//                new Goal { Id = 4, GoalId = "g4", Title = "Form a Study Group", Points = 25 },
//                new Goal { Id = 5, GoalId = "g5", Title = "Study for 30 mins", Points = 35 },
//                new Goal { Id = 6, GoalId = "g6", Title = "Take 5 Notes", Points = 20 }
//            );
//        }
//    }
//}