using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace MockTrialTabulation.Database
{
    enum state
    {
        PRELIMMINARY,
        ROUND1,
        ROUND2,
        ROUND3,
        ROUND4,
        AWARDS
    }

    enum judge_value
    {
        A,
        B
    }

    public enum last_side : Int64
    {
        A = 1,
        B = 2
    }

    class score
    {
        public team team { get; set; }
        public Decimal ballots { get; set; }
        public Decimal combined_strength { get; set; }
        public Int64 point_differntial { get; set; }
        public last_side last_side { get; set; }
    }

    class team
    {
        public Int64 id { get; set; }
        public String name { get; set; }
        public String obfuscated_name { get; set; }
        public Int64 team_number { get; set; }
        public Int64 known_conflict1 { get; set; }
        public Int64 known_conflict2 { get; set; }
        public Int64 known_conflict3 { get; set; }
        public Int64 known_conflict4 { get; set; }
        public Int64 known_conflict5 { get; set; }
        public Int64 known_conflict6 { get; set; }
    }

    class pretty_team
    {
        public team team { get; set; }
        public List<student> students { get; set; }
    }

    class judge
    {
        public Int64 id { get; set; }
        public String name { get; set; }
        public Int64 known_conflict1 { get; set; }
        public Int64 known_conflict2 { get; set; }
        public Int64 known_conflict3 { get; set; }
        public Int64 known_conflict4 { get; set; }
        public Int64 known_conflict5 { get; set; }
        public Int64 known_conflict6 { get; set; }
        public Int64 known_conflict7 { get; set; }
        public Int64 known_conflict8 { get; set; }
        public Int64 known_conflict9 { get; set; }
        public Int64 known_conflict10 { get; set; }
        public bool active_round_1 { get; set; }
        public bool active_round_2 { get; set; }
        public bool active_round_3 { get; set; }
        public bool active_round_4 { get; set; }
        public Int64 affinity_1 { get; set; }
        public Int64 affinity_2 { get; set; }
    }
    class pretty_student
    {
        public student student { get; set; }
        public team team { get; set; }
        public String pretty
        {
            get
            {
                return String.Format("name={0}, side={1}, team={2}", this.student.name, this.student.side, this.team.name);
            }
        }
        public Int64 points { get; set; }
    }
    class student
    {
        public Int64 id { get; set; }
        public String name { get; set; }
        public String side { get; set; }
        public Int64 team { get; set; }
    }

    class rank
    {
        public Int64 id { get; set; }
        public Int64 witness1 { get; set; }
        public Int64 witness2 { get; set; }
        public Int64 witness3 { get; set; }
        public Int64 witness4 { get; set; }
        public Int64 attorney1 { get; set; }
        public Int64 attorney2 { get; set; }
        public Int64 attorney3 { get; set; }
        public Int64 attorney4 { get; set; }
    }

    class round
    {
        public Int64 id { get; set; }
        public Int64 round_number { get; set; }
        public Int64 team_a { get; set; }
        public Int64 team_b { get; set; }
        public Int64 judge_a { get; set; }
        public Int64 judge_b { get; set; }
        public Int64 judge_c { get; set; }
        public Int64 judge_a_score_team_a { get; set; }
        public Int64 judge_a_score_team_b { get; set; }
        public Int64 judge_b_score_team_a { get; set; }
        public Int64 judge_b_score_team_b { get; set; }
        public Int64 judge_a_ranks { get; set; }
        public Int64 judge_b_ranks { get; set; }
    }
    class pretty_round
    {
        public round round;
        public team team_a { get; set; }
        public team team_b { get; set; }
        public judge judge_a { get; set; }
        public judge judge_b { get; set; }
        public judge judge_c { get; set; }
        public String pretty {
            get
            {
                if (judge_c != null)
                {
                    return String.Format("round_number={0}, team_a={1}, team_b={2}, judge_a={3}, judge_b={4}, judge_c={5}", round.round_number, team_a.name, team_b.name, judge_a.name, judge_b.name, judge_c.name);
                }
                if (judge_b != null)
                {
                    return String.Format("round_number={0}, team_a={1}, team_b={2}, judge_a={3}, judge_b={4}", round.round_number, team_a.name, team_b.name, judge_a.name, judge_b.name);
                }
                return String.Format("round_number={0}, team_a={1}, team_b={2}, judge_a={3}", round.round_number, team_a.name, team_b.name, judge_a.name);
            }
        }
    }
    class Database : IDisposable
    {
        public const String pathname = "tabulation.sqlite";
        public SQLiteConnection connection;
        public Database()
        {
            if (!File.Exists(pathname))
            {
                SQLiteConnection.CreateFile(pathname);
            }
            connection = new SQLiteConnection(String.Format("Data Source={0};Version=3;", pathname));
            connection.Open();
            String tables = @"CREATE TABLE IF NOT EXISTS
            teams (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                team_number INTEGER,
                obfuscated_name TEXT,
                known_conflict1 INTEGER,
                known_conflict2 INTEGER,
                known_conflict3 INTEGER,
                known_conflict4 INTEGER,
                known_conflict5 INTEGER,
                known_conflict6 INTEGER,
                FOREIGN KEY(known_conflict1) REFERENCES teams(id),
                FOREIGN KEY(known_conflict2) REFERENCES teams(id),
                FOREIGN KEY(known_conflict3) REFERENCES teams(id),
                FOREIGN KEY(known_conflict4) REFERENCES teams(id),
                FOREIGN KEY(known_conflict5) REFERENCES teams(id),
                FOREIGN KEY(known_conflict6) REFERENCES teams(id)
            );
            CREATE TABLE IF NOT EXISTS
            judges (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                known_conflict1 INTEGER,
                known_conflict2 INTEGER,
                known_conflict3 INTEGER,
                known_conflict4 INTEGER,
                known_conflict5 INTEGER,
                known_conflict6 INTEGER,
                known_conflict7 INTEGER,
                known_conflict8 INTEGER,
                known_conflict9 INTEGER,
                known_conflict10 INTEGER,
                active_round_1 INTEGER,
                active_round_2 INTEGER,
                active_round_3 INTEGER,
                active_round_4 INTEGER,
                affinity_1 INTEGER,
                affinity_2 INTEGER,
                FOREIGN KEY(known_conflict1) REFERENCES teams(id),
                FOREIGN KEY(known_conflict2) REFERENCES teams(id),
                FOREIGN KEY(known_conflict3) REFERENCES teams(id),
                FOREIGN KEY(known_conflict4) REFERENCES teams(id),
                FOREIGN KEY(known_conflict5) REFERENCES teams(id),
                FOREIGN KEY(known_conflict6) REFERENCES teams(id),
                FOREIGN KEY(known_conflict7) REFERENCES teams(id),
                FOREIGN KEY(known_conflict8) REFERENCES teams(id),
                FOREIGN KEY(known_conflict9) REFERENCES teams(id),
                FOREIGN KEY(known_conflict10) REFERENCES teams(id),
                FOREIGN KEY(affinity_1) REFERENCES judges(id),
                FOREIGN KEY(affinity_2) REFERENCES judges(id)
            );
            CREATE TABLE IF NOT EXISTS
            students (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                side TEXT NOT NULL,
                team INTEGER NOT NULL,
                FOREIGN KEY(team) REFERENCES teams(id)
            );
            CREATE TABLE IF NOT EXISTS
            ranks (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                witness1 INTEGER NOT NULL,
                witness2 INTEGER NOT NULL,
                witness3 INTEGER NOT NULL,
                witness4 INTEGER NOT NULL,
                attorney1 INTEGER NOT NULL,
                attorney2 INTEGER NOT NULL,
                attorney3 INTEGER NOT NULL,
                attorney4 INTEGER NOT NULL,
                FOREIGN KEY (witness1) REFERENCES students(id),
                FOREIGN KEY (witness2) REFERENCES students(id),
                FOREIGN KEY (witness3) REFERENCES students(id),
                FOREIGN KEY (witness4) REFERENCES students(id),
                FOREIGN KEY (attorney1) REFERENCES students(id),
                FOREIGN KEY (attorney2) REFERENCES students(id),
                FOREIGN KEY (attorney3) REFERENCES students(id),
                FOREIGN KEY (attorney4) REFERENCES students(id)
            );
            CREATE TABLE IF NOT EXISTS
            rounds (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                round_number INTEGER NOT NULL,
                team_a INTEGER NOT NULL,
                team_b INTEGER NOT NULL,
                judge_a INTEGER NOT NULL,
                judge_b INTEGER,
                judge_c INTEGER,
                judge_a_score_team_a INTEGER,
                judge_a_score_team_b INTEGER,
                judge_b_score_team_a INTEGER,
                judge_b_score_team_b INTEGER,
                judge_a_ranks INTEGER,
                judge_b_ranks INTEGER,
                FOREIGN KEY(team_a) REFERENCES teams(id),
                FOREIGN KEY(team_b) REFERENCES teams(id),
                FOREIGN KEY(judge_a) REFERENCES judges(id),
                FOREIGN KEY(judge_b) REFERENCES judges(id),
                FOREIGN KEY(judge_c) REFERENCES judges(id),
                FOREIGN KEY(judge_a_ranks) REFERENCES ranks(id),
                FOREIGN KEY(judge_b_ranks) REFERENCES ranks(id)
            );";
            SQLiteCommand command = new SQLiteCommand(tables, connection);
            command.ExecuteNonQuery();
        }

        public void Close()
        {
            if (connection != null)
            {
                connection.Close();
            }
        }

        public bool AwardsReady()
        {
            String query = "SELECT COUNT(id) FROM rounds WHERE judge_a_ranks IS NULL OR (judge_b IS NOT NULL AND judge_b_ranks IS NULL)";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                if (result.GetType() == typeof(DBNull))
                {
                    return false;
                }
                Int64 number = Convert.ToInt64(result);
                return number == 0 && GetState() == state.ROUND4;
            }
        }

        public List<score> GetScores(Int64 make_for_round, List<team> teams)
        {
            if (make_for_round > 4)
            {
                Exception ex = new Exception("Cannot make scores for a round greater than 4");
                throw ex;
            }
            if (make_for_round == 1)
            {
                Exception ex = new Exception("Scores are not necessary for round 1");
                throw ex;
            }
            var rounds = GetRounds();
            List<score> scores = new List<score>();
            Dictionary<Int64, Decimal> ballots = new Dictionary<Int64, Decimal>();
            Dictionary<Int64, Int64> point_differntial = new Dictionary<Int64, Int64>();
            Dictionary<Int64, Tuple<Int64, Int64>> last_sides = new Dictionary<Int64, Tuple<Int64, Int64>>();
            foreach (round round in rounds)
            {
                //Init zero ballots
                if (!ballots.ContainsKey(round.team_a))
                {
                    ballots[round.team_a] = 0m;
                    point_differntial[round.team_a] = 0;
                }
                if(!ballots.ContainsKey(round.team_b))
                {
                    ballots[round.team_b] = 0m;
                    point_differntial[round.team_b] = 0;
                }
                if (!last_sides.ContainsKey(round.team_a))
                {
                    last_sides[round.team_a] = new Tuple<Int64, Int64>(round.round_number, (Int64)last_side.A);
                }
                else
                {
                    if (last_sides[round.team_a].Item2 < round.round_number)
                    {
                        last_sides[round.team_a] = new Tuple<Int64, Int64>(round.round_number, (Int64)last_side.A);
                    }
                }

                if (!last_sides.ContainsKey(round.team_b))
                {
                    last_sides[round.team_b] = new Tuple<Int64, Int64>(round.round_number, (Int64)last_side.B);
                }
                else
                {
                    if (last_sides[round.team_b].Item2 < round.round_number)
                    {
                        last_sides[round.team_b] = new Tuple<Int64, Int64>(round.round_number, (Int64)last_side.B);
                    }
                }
                // One judge
                if (round.judge_b == 0)
                {
                    if (round.judge_a_score_team_a > round.judge_a_score_team_b)
                    {
                        ballots[round.team_a] += 2m;
                        Int64 difference = (round.judge_a_score_team_a - round.judge_a_score_team_b)*2;
                        point_differntial[round.team_a] += difference;
                        point_differntial[round.team_b] -= difference;
                    }
                    else if (round.judge_a_score_team_a == round.judge_a_score_team_b)
                    {
                        ballots[round.team_a] += 1m;
                        ballots[round.team_b] += 1m;
                    }
                    else
                    {
                        //Implies team b won
                        ballots[round.team_b] += 2m;
                        Int64 difference = (round.judge_a_score_team_b - round.judge_a_score_team_a) * 2;
                        point_differntial[round.team_b] += difference;
                        point_differntial[round.team_a] -= difference;
                    }
                }
                else
                {
                    //Two judges
                    //Judge A
                    if (round.judge_a_score_team_a > round.judge_a_score_team_b)
                    {
                        ballots[round.team_a] += 1m;
                        Int64 difference = (round.judge_a_score_team_a - round.judge_a_score_team_b);
                        point_differntial[round.team_a] += difference;
                        point_differntial[round.team_b] -= difference;
                    }
                    else if (round.judge_a_score_team_a == round.judge_a_score_team_b)
                    {
                        ballots[round.team_a] += 0.5m;
                        ballots[round.team_b] += 0.5m;
                    }
                    else
                    {
                        //Implies team b won
                        ballots[round.team_b] += 1m;
                        Int64 difference = (round.judge_a_score_team_b - round.judge_a_score_team_a);
                        point_differntial[round.team_b] += difference;
                        point_differntial[round.team_a] -= difference;
                    }
                    //Judge B
                    if (round.judge_b_score_team_a > round.judge_b_score_team_b)
                    {
                        ballots[round.team_a] += 1m;
                        Int64 difference = (round.judge_b_score_team_a - round.judge_b_score_team_b);
                        point_differntial[round.team_a] += difference;
                        point_differntial[round.team_b] -= difference;
                    }
                    else if (round.judge_b_score_team_a == round.judge_b_score_team_b)
                    {
                        ballots[round.team_a] += 0.5m;
                        ballots[round.team_b] += 0.5m;
                    }
                    else
                    {
                        //Implies team b won
                        ballots[round.team_b] += 1m;
                        Int64 difference = (round.judge_b_score_team_b - round.judge_b_score_team_a);
                        point_differntial[round.team_b] += difference;
                        point_differntial[round.team_a] -= difference;
                    }
                }
            }
            Dictionary<Int64, List<Int64>> opponents = new Dictionary<Int64, List<Int64>>();
            Dictionary<Int64, Decimal> combined_strength = new Dictionary<Int64, Decimal>();
            foreach (team team in teams)
            {
                opponents.Add(team.id, new List<Int64>());
            }
            foreach(round round in rounds)
            {
                Int64 team_a_id = round.team_a;
                Int64 team_b_id = round.team_b;
                opponents[team_a_id].Add(team_b_id);
                opponents[team_b_id].Add(team_a_id);
            }
            if (make_for_round > 2)
            {
                foreach (Int64 team in opponents.Keys)
                {
                    foreach (Int64 opponent in opponents[team])
                    {
                        if (!combined_strength.ContainsKey(team))
                        {
                            combined_strength[team] = 0;
                        }
                        combined_strength[team] += ballots[opponent];
                    }
                }
            }
            foreach (team team in teams)
            {
                if (make_for_round > 2)
                {
                    scores.Add(new score { team = team, ballots = ballots[team.id], combined_strength = combined_strength[team.id], point_differntial = point_differntial[team.id], last_side = (last_side)last_sides[team.id].Item2 });
                }
                else
                {
                    scores.Add(new score { team = team, ballots = ballots[team.id], point_differntial = point_differntial[team.id], last_side=(last_side)last_sides[team.id].Item2});
                }
            }
            List<score> sorted;
            if (make_for_round > 2)
            {
                sorted = scores.OrderBy(o => o.ballots).ThenBy(o => o.combined_strength).ThenBy(o => o.point_differntial).ToList();
            }
            else
            {
                sorted = scores.OrderBy(o => o.ballots).ThenBy(o => o.point_differntial).ToList();
            }
            return sorted;
        }

        public String GetTeamNameFromID(Int64 id)
        {
            String query = String.Format("SELECT name FROM teams WHERE id = {0} LIMIT 1", id);
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                return result.ToString();
            }
        }

        public Int64 GetRankID(Int64 witness1, Int64 witness2, Int64 witness3, Int64 witness4, Int64 attorney1, Int64 attorney2, Int64 attorney3, Int64 attorney4)
        {
            String query = String.Format("SELECT id FROM ranks WHERE witness1 = {0} AND witness2 = {1} AND witness3 = {2} AND witness4 = {3} AND attorney1 = {4} AND attorney2 = {5} AND attorney3 = {6} AND attorney4 = {7} LIMIT 1", witness1, witness2, witness3, witness4, attorney1, attorney2, attorney3, attorney4);
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                return Convert.ToInt64(result.ToString());
            }
        }
        public void AddRankID(round round, Int64 id, judge_value value)
        {
            String judge_string = value == judge_value.A ? "a" : "b";
            String query = String.Format("UPDATE rounds SET judge_{0}_ranks = {1} WHERE id = {2};", judge_string, id, round.id);
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        public void AddScore(round round, Int64 team_a_score, Int64 team_b_score, judge_value value)
        {
            String judge_string = value == judge_value.A ? "a" : "b";
            String query = String.Format("UPDATE rounds SET judge_{0}_score_team_a = {1} WHERE id = {2}", judge_string, team_a_score, round.id);
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
            String query2 = String.Format("UPDATE rounds SET judge_{0}_score_team_b = {1} WHERE id = {2}", judge_string, team_b_score, round.id);
            using (SQLiteCommand command = new SQLiteCommand(query2, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public List<student> GetStudentsFromTeamID(Int64 id)
        {
            List<student> students = new List<student>();
            String query = String.Format("SELECT * from students WHERE team = {0}", id);
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        student student = ReaderToStudent(reader);
                        students.Add(student);
                    }
                }
            }
            return students;
        }
        public List<student> GetStudents()
        {
            List<student> students = new List<student>();
            String query = String.Format("SELECT * from students");
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        student student = ReaderToStudent(reader);
                        students.Add(student);
                    }
                }
            }
            return students;
        }
        public state GetState()
        {
            String query = "SELECT MAX(DISTINCT round_number) FROM rounds";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                if (result.GetType() == typeof(DBNull))
                {
                    return state.PRELIMMINARY;
                }
                Int64 number = Convert.ToInt64(result);
                if (number == 1)
                {
                    return state.ROUND1;
                }
                else if (number == 2)
                {
                    return state.ROUND2;
                }
                else if (number == 3)
                {
                    return state.ROUND3;
                }
                else if (number == 4)
                {
                    return state.ROUND4;
                }
                else
                {
                    Exception ex = new Exception("This round number should never exist");
                    throw ex;
                }
            }
        }

        private student ReaderToStudent(SQLiteDataReader reader)
        {
            Int64 id = Convert.ToInt64(reader["id"]);
            String name = Convert.ToString(reader["name"]);
            String side = Convert.ToString(reader["side"]);
            Int64 team = Convert.ToInt64(reader["team"]);
            return new student { id = id, name = name, side = side, team = team };
        }

        private team ReaderToTeam(SQLiteDataReader reader)
        {
            Int64 id = Convert.ToInt64(reader["id"]);
            Int64 number = 0;
            if (reader["team_number"].GetType() != typeof(System.DBNull))
            {
                number = Convert.ToInt64(reader["team_number"]);
            }
            Int64 conflict1 = 0;
            if (reader["known_conflict1"].GetType() != typeof(System.DBNull))
            {
                conflict1 = Convert.ToInt64(reader["known_conflict1"]);
            }
            Int64 conflict2 = 0;
            if (reader["known_conflict2"].GetType() != typeof(System.DBNull))
            {
                conflict2 = Convert.ToInt64(reader["known_conflict2"]);
            }
            Int64 conflict3 = 0;
            if (reader["known_conflict3"].GetType() != typeof(System.DBNull))
            {
                conflict3 = Convert.ToInt64(reader["known_conflict3"]);
            }
            Int64 conflict4 = 0;
            if (reader["known_conflict4"].GetType() != typeof(System.DBNull))
            {
                conflict4 = Convert.ToInt64(reader["known_conflict4"]);
            }
            Int64 conflict5 = 0;
            if (reader["known_conflict5"].GetType() != typeof(System.DBNull))
            {
                conflict5 = Convert.ToInt64(reader["known_conflict5"]);
            }
            Int64 conflict6 = 0;
            if (reader["known_conflict6"].GetType() != typeof(System.DBNull))
            {
                conflict6 = Convert.ToInt64(reader["known_conflict6"]);
            }
            return new team { id = id, name = reader.GetString(1), obfuscated_name = reader.GetString(3), team_number = number, known_conflict1 = conflict1, known_conflict2 = conflict2, known_conflict3 = conflict3, known_conflict4 = conflict4, known_conflict5 = conflict5, known_conflict6 = conflict6 };
        }

        public judge ReaderToJudge(SQLiteDataReader reader)
        {
            Int64 id = Convert.ToInt64(reader["id"]);
            Int64 conflict1 = 0;
            if (reader["known_conflict1"].GetType() != typeof(System.DBNull))
            {
                conflict1 = Convert.ToInt64(reader["known_conflict1"]);
            }
            Int64 conflict2 = 0;
            if (reader["known_conflict2"].GetType() != typeof(System.DBNull))
            {
                conflict2 = Convert.ToInt64(reader["known_conflict2"]);
            }
            Int64 conflict3 = 0;
            if (reader["known_conflict3"].GetType() != typeof(System.DBNull))
            {
                conflict3 = Convert.ToInt64(reader["known_conflict3"]);
            }
            Int64 conflict4 = 0;
            if (reader["known_conflict4"].GetType() != typeof(System.DBNull))
            {
                conflict4 = Convert.ToInt64(reader["known_conflict4"]);
            }
            Int64 conflict5 = 0;
            if (reader["known_conflict5"].GetType() != typeof(System.DBNull))
            {
                conflict5 = Convert.ToInt64(reader["known_conflict5"]);
            }
            Int64 conflict6 = 0;
            if (reader["known_conflict6"].GetType() != typeof(System.DBNull))
            {
                conflict6 = Convert.ToInt64(reader["known_conflict6"]);
            }
            Int64 conflict7 = 0;
            if (reader["known_conflict7"].GetType() != typeof(System.DBNull))
            {
                conflict7 = Convert.ToInt64(reader["known_conflict7"]);
            }
            Int64 conflict8 = 0;
            if (reader["known_conflict8"].GetType() != typeof(System.DBNull))
            {
                conflict8 = Convert.ToInt64(reader["known_conflict8"]);
            }
            Int64 conflict9 = 0;
            if (reader["known_conflict9"].GetType() != typeof(System.DBNull))
            {
                conflict9 = Convert.ToInt64(reader["known_conflict9"]);
            }
            Int64 conflict10 = 0;
            if (reader["known_conflict10"].GetType() != typeof(System.DBNull))
            {
                conflict10 = Convert.ToInt64(reader["known_conflict10"]);
            }
            bool active_round_1 = false;
            if (reader["active_round_1"].GetType() != typeof(System.DBNull))
            {
                active_round_1 = Convert.ToInt64(reader["active_round_1"]) == 0 ? false : true;
            }
            bool active_round_2 = false;
            if (reader["active_round_2"].GetType() != typeof(System.DBNull))
            {
                active_round_2 = Convert.ToInt64(reader["active_round_2"]) == 0 ? false : true;
            }
            bool active_round_3 = false;
            if (reader["active_round_3"].GetType() != typeof(System.DBNull))
            {
                active_round_3 = Convert.ToInt64(reader["active_round_3"]) == 0 ? false : true;
            }
            bool active_round_4 = false;
            if (reader["active_round_4"].GetType() != typeof(System.DBNull))
            {
                active_round_4 = Convert.ToInt64(reader["active_round_4"]) == 0 ? false : true;
            }
            Int64 affinity_1 = 0;
            if (reader["affinity_1"].GetType() != typeof(System.DBNull))
            {
                affinity_1 = Convert.ToInt64(reader["affinity_1"]);
            }
            Int64 affinity_2 = 0;
            if (reader["affinity_2"].GetType() != typeof(System.DBNull))
            {
                affinity_2 = Convert.ToInt64(reader["affinity_2"]);
            }
            return new judge { id = id, name = reader.GetString(1), known_conflict1 = conflict1, known_conflict2 = conflict2, known_conflict3 = conflict3, known_conflict4 = conflict4, known_conflict5 = conflict5, known_conflict6 = conflict6, known_conflict7 = conflict7, known_conflict8 = conflict8, known_conflict9 = conflict9, known_conflict10 = conflict10, active_round_1 = active_round_1, active_round_2=active_round_2, active_round_3=active_round_3, active_round_4=active_round_4, affinity_1=affinity_1, affinity_2=affinity_2};
        }

        public List<team> GetTeams()
        {
            List<team> teams = new List<team>();
            String query = "SELECT * FROM teams";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        team team = ReaderToTeam(reader);
                        teams.Add(team);
                    }
                }
            }
            return teams;
        }

        public List<judge> GetJudges(Int64 round = 0)
        {
            List<judge> judges= new List<judge>();
            String query = round == 0 ? "SELECT * FROM judges" : String.Format("SELECT * FROM judges WHERE active_round_{0} = 1", round);
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        judge judge = ReaderToJudge(reader);
                        judges.Add(judge);
                    }
                }
            }
            return judges;
        }

        public round ReaderToRound(SQLiteDataReader reader)
        {
            Int64 id = Convert.ToInt64(reader["id"]);
            Int64 round_number = Convert.ToInt64(reader["round_number"]);
            Int64 team_a = Convert.ToInt64(reader["team_a"]);
            Int64 team_b = Convert.ToInt64(reader["team_b"]);
            Int64 judge_a = Convert.ToInt64(reader["judge_a"]);
            Int64 judge_b = 0;
            if (reader["judge_b"].GetType() != typeof(System.DBNull))
            {
                judge_b = Convert.ToInt64(reader["judge_b"]);
            }
            Int64 judge_c = 0;
            if (reader["judge_c"].GetType() != typeof(System.DBNull))
            {
                judge_c = Convert.ToInt64(reader["judge_c"]);
            }
            Int64 judge_a_score_team_a = 0;
            if (reader["judge_a_score_team_a"].GetType() != typeof(System.DBNull))
            {
                judge_a_score_team_a = Convert.ToInt64(reader["judge_a_score_team_a"]);
            }
            Int64 judge_b_score_team_a = 0;
            if (reader["judge_b_score_team_a"].GetType() != typeof(System.DBNull))
            {
                judge_b_score_team_a = Convert.ToInt64(reader["judge_b_score_team_a"]);
            }
            Int64 judge_a_score_team_b = 0;
            if (reader["judge_a_score_team_b"].GetType() != typeof(System.DBNull))
            {
                judge_a_score_team_b = Convert.ToInt64(reader["judge_a_score_team_b"]);
            }
            Int64 judge_b_score_team_b = 0;
            if (reader["judge_b_score_team_b"].GetType() != typeof(System.DBNull))
            {
                judge_b_score_team_b = Convert.ToInt64(reader["judge_b_score_team_b"]);
            }

            Int64 judge_a_ranks = 0;
            if (reader["judge_a_ranks"].GetType() != typeof(System.DBNull))
            {
                judge_a_ranks = Convert.ToInt64(reader["judge_a_ranks"]);
            }
            Int64 judge_b_ranks = 0;
            if (reader["judge_b_ranks"].GetType() != typeof(System.DBNull))
            {
                judge_b_ranks = Convert.ToInt64(reader["judge_b_ranks"]);
            }
            return new round {id=id, round_number=round_number, team_a=team_a, team_b=team_b, judge_a=judge_a, judge_b=judge_b, judge_c=judge_c, judge_a_score_team_a = judge_a_score_team_a, judge_a_score_team_b=judge_a_score_team_b, judge_b_score_team_a=judge_b_score_team_a, judge_b_score_team_b=judge_a_score_team_b,judge_a_ranks= judge_a_ranks , judge_b_ranks= judge_b_ranks };
        }

        public List<round> GetRounds(Int64 round_number = 0)
        {
            List<round> rounds = new List<round>();
            String query = "SELECT * FROM rounds";
            if (round_number != 0)
            {
                query = String.Format("SELECT * FROM rounds WHERE round_number = {0}", round_number);
            }
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        round round = ReaderToRound(reader);
                        rounds.Add(round);
                    }
                }
            }
            return rounds;
        }

        public rank ReaderToRank(SQLiteDataReader reader)
        {
            Int64 id = Convert.ToInt64(reader["id"]);
            Int64 witness1 = Convert.ToInt64(reader["witness1"]);
            Int64 witness2 = Convert.ToInt64(reader["witness2"]);
            Int64 witness3 = Convert.ToInt64(reader["witness3"]);
            Int64 witness4 = Convert.ToInt64(reader["witness4"]);
            Int64 attorney1 = Convert.ToInt64(reader["attorney1"]);
            Int64 attorney2 = Convert.ToInt64(reader["attorney2"]);
            Int64 attorney3 = Convert.ToInt64(reader["attorney3"]);
            Int64 attorney4 = Convert.ToInt64(reader["attorney4"]);
            return new rank { id=id, witness1=witness1, witness2=witness2, witness3=witness3, witness4=witness4, attorney1=attorney1, attorney2=attorney2, attorney3=attorney3, attorney4=attorney4};
        }

        public List<rank> GetRanks()
        {
            List<rank> ranks = new List<rank>();
            String query = "SELECT * FROM ranks";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rank rank = ReaderToRank(reader);
                        ranks.Add(rank);
                    }
                }
            }
            return ranks;
        }

        private String NullOrString<InputType>(InputType input)
        {
            if (input.ToString() == "NULL")
            {
                return "NULL";
            }
            return String.Format("'{0}'", input);
        }
        
        private String NullForZero(Int64 input)
        {
            if (input == 0)
            {
                return "NULL";
            }
            return String.Format("'{0}'", input);
        }

        private void SafeTransaction(String command)
        {
            using (var cmd = new SQLiteCommand(connection))
            {
                using (var transaction = connection.BeginTransaction())
                {
                    cmd.CommandText = command;
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }
        public Int64 BoolToInt(bool value)
        {
            if (value)
            {
                return 1;
            }
            return 0;
        }
        public void InsertJudge(String name, Int64 known_conflict1 = 0, Int64 known_conflict2 = 0, Int64 known_conflict3 = 0, Int64 known_conflict4 = 0, Int64 known_conflict5 = 0, Int64 known_conflict6 = 0, Int64 known_conflict7 = 0, Int64 known_conflict8 = 0, Int64 known_conflict9 = 0, Int64 known_conflict10 = 0, bool active_round_1=false, bool active_round_2 = false, bool active_round_3=false, bool active_round_4=false, Int64 affinity_1=0, Int64 affinity_2=0)
        {
            String command = String.Format("INSERT INTO judges (name, known_conflict1, known_conflict2, known_conflict3, known_conflict4, known_conflict5, known_conflict6, known_conflict7, known_conflict8, known_conflict9, known_conflict10, active_round_1, active_round_2, active_round_3, active_round_4, affinity_1, affinity_2) VALUES ('{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16})", name, NullForZero(known_conflict1), NullForZero(known_conflict2), NullForZero(known_conflict3), NullForZero(known_conflict4), NullForZero(known_conflict5), NullForZero(known_conflict6), NullForZero(known_conflict7), NullForZero(known_conflict8), NullForZero(known_conflict9), NullForZero(known_conflict10), BoolToInt(active_round_1), BoolToInt(active_round_2), BoolToInt(active_round_3), BoolToInt(active_round_4), NullForZero(affinity_1), NullForZero(affinity_2));
            SafeTransaction(command);
        }

        public void InsertTeam(String name, String obfuscated_name = "NULL", Int64 team_number = 0, Int64 known_conflict1 = 0, Int64 known_conflict2 = 0, Int64 known_conflict3 = 0, Int64 known_conflict4 = 0, Int64 known_conflict5 = 0, Int64 known_conflict6 = 0)
        {
            String command = String.Format("INSERT INTO teams (name, obfuscated_name, team_number, known_conflict1, known_conflict2, known_conflict3, known_conflict4, known_conflict5, known_conflict6) VALUES ('{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})", name, NullOrString(obfuscated_name), NullForZero(team_number), NullForZero(known_conflict1), NullForZero(known_conflict2), NullForZero(known_conflict3), NullForZero(known_conflict4), NullForZero(known_conflict5), NullForZero(known_conflict6));
            SafeTransaction(command);
        }

        public void InsertStudent(String name, String side, Int64 team)
        {
            String command = String.Format("INSERT INTO students (name, side, team) VALUES ('{0}', '{1}', '{2}')", name, side, team);
            SafeTransaction(command);
        }

        public void InsertRank(Int64 witness1, Int64 witness2, Int64 witness3, Int64 witness4, Int64 attorney1, Int64 attorney2, Int64 attorney3, Int64 attorney4)
        {
            String command = String.Format("INSERT INTO ranks (witness1, witness2, witness3, witness4, attorney1, attorney2, attorney3, attorney4) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", witness1, witness2, witness3, witness4, attorney1, attorney2, attorney3, attorney4);
            SafeTransaction(command);
        }
        
        public void InsertRound(Int64 round_number, Int64 team_a, Int64 team_b, Int64 judge_a, Int64 judge_b = 0, Int64 judge_c = 0)
        {
            String command = String.Format("INSERT INTO rounds (round_number, team_a, team_b, judge_a, judge_b, judge_c) VALUES ('{0}', '{1}', '{2}', '{3}', {4}, {5})", round_number, team_a, team_b, judge_a, NullForZero(judge_b), NullForZero(judge_c));
            SafeTransaction(command);
        }

        public void AddTeamConflict(Int64 team_id, Int64 conflict_id)
        {
            team team = GetTeamFromID(team_id);
            Int64 next_available_conflict = 0;
            if (team.known_conflict1 == 0)
            {
                next_available_conflict = 1;
            }
            else if (team.known_conflict2 == 0)
            {
                next_available_conflict = 2;
            }
            else if (team.known_conflict3 == 0)
            {
                next_available_conflict = 3;
            }
            else if (team.known_conflict4 == 0)
            {
                next_available_conflict = 4;
            }
            else if (team.known_conflict5 == 0)
            {
                next_available_conflict = 5;
            }
            else if (team.known_conflict6 == 0)
            {
                next_available_conflict = 6;
            }
            if (next_available_conflict == 0)
            {
                Exception ex = new Exception("Too many team conflicts to add another");
                throw ex;
            }
            String query = String.Format("UPDATE teams SET known_conflict{0} = {1} WHERE id = {2}", next_available_conflict, conflict_id, team.id);
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }

        }

        public void AddJudgeConflict(Int64 judge_id, Int64 conflict_id)
        {
            judge judge = GetJudgeFromID(judge_id);
            Int64 next_available_conflict = 0;
            if (judge.known_conflict1 == 0)
            {
                next_available_conflict = 1;
            }
            else if (judge.known_conflict2 == 0)
            {
                next_available_conflict = 2;
            }
            else if (judge.known_conflict3 == 0)
            {
                next_available_conflict = 3;
            }
            else if (judge.known_conflict4 == 0)
            {
                next_available_conflict = 4;
            }
            else if (judge.known_conflict5 == 0)
            {
                next_available_conflict = 5;
            }
            else if (judge.known_conflict6 == 0)
            {
                next_available_conflict = 6;
            }
            else if (judge.known_conflict7 == 0)
            {
                next_available_conflict = 7;
            }
            else if (judge.known_conflict8 == 0)
            {
                next_available_conflict = 8;
            }
            else if (judge.known_conflict9 == 0)
            {
                next_available_conflict = 9;
            }
            else if (judge.known_conflict10 == 0)
            {
                next_available_conflict = 10;
            }
            if (next_available_conflict == 0)
            {
                Exception ex = new Exception("Too many team conflicts to add another");
                throw ex;
            }
            String query = String.Format("UPDATE judges SET known_conflict{0} = {1} WHERE id = {2}", next_available_conflict, conflict_id, judge.id);
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public team GetTeamFromID(Int64 id)
        {
            String query = String.Format("SELECT * FROM teams where id = {0} LIMIT 1", id);
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        team team = ReaderToTeam(reader);
                        return team;
                    }
                }
            }
            return new team();
        }

        public judge GetJudgeFromID(Int64 id)
        {
            String query = String.Format("SELECT * FROM judges where id = {0} LIMIT 1", id);
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        judge judge = ReaderToJudge(reader);
                        return judge;
                    }
                }
            }
            return new judge();
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Close();
            }
        }
    }
}
