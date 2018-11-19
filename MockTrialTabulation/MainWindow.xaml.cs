using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using MockTrialTabulation.Database;
using System.Security.Cryptography;
using System.IO;

namespace MockTrialTabulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Database.Database database = new Database.Database();
        private state state = state.PRELIMMINARY;
        private bool winners_loaded = false;
        private bool personal_winners_loaded = false;
        private bool prompted = false;
        public MainWindow()
        {
            if (!prompted)
            {
                prompted = true;
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Do you want to start with a fresh database?", "Clear and start fresh", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (File.Exists(Database.Database.pathname))
                    {
                        database.Close();
                        File.Delete(Database.Database.pathname);
                        database = new Database.Database();
                    }
                }
            }
            List<team> teams = database.GetTeams();
            var state = database.GetState();
            this.state = state;
            InitializeComponent();
            if (teams.Count != 0)
            {
                this.listViewTeamsForTeams.ItemsSource = teams;
            }
            this.labelStateGenerate.Content = stateToText();
            this.labelStateEnter.Content = stateToText();
            this.comboBoxRoundNumber.ItemsSource = new List<string> { "1", "2", "3", "4" };
        }
        
        private void Warn(String context, String reason)
        {
            String message = String.Format("Issue with {0} validation: {1}", context, reason);
            MessageBox.Show(message);
        }

        public static List<T> Shuffle<T>(List<T> input)
        {
            List<T> list = input;
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        private void Success(String context, String reason)
        {
            String message = String.IsNullOrEmpty(reason) ? String.Format("Success performing action: {0}", context) : String.Format("Success performing action: {0}, {1}", context, reason);
            MessageBox.Show(message);

        }

        private pretty_student StudentToPrettyStudent(student student)
        {
            team team = database.GetTeamFromID(student.team);
            return new pretty_student { student = student, team = team };
        }

        private pretty_round RoundToPrettyRound(round round)
        {
            team team_a = database.GetTeamFromID(round.team_a);
            team team_b = database.GetTeamFromID(round.team_b);
            judge judge_a = database.GetJudgeFromID(round.judge_a);
            judge judge_b = database.GetJudgeFromID(round.judge_b);
            judge judge_c = database.GetJudgeFromID(round.judge_c);
            return new pretty_round { round = round , team_a=team_a, team_b=team_b, judge_a=judge_a, judge_b=judge_b, judge_c=judge_c};
        }

        private pretty_team TeamToPrettyTeam(team team)
        {
            List<student> students = database.GetStudentsFromTeamID(team.id);
            return new pretty_team { team=team, students=students};
        }

        private String stateToText()
        {
            if (this.state == state.PRELIMMINARY)
            {
                return "Preliminary State, next is Round 1";
            }
            if (this.state == state.ROUND1)
            {
                return "Round 1 State, next is Round 2";
            }
            if (this.state == state.ROUND2)
            {
                return "Round 2 State, next is Round 3";
            }
            if (this.state == state.ROUND3)
            {
                return "Round 3 State, next is Round 4";
            }
            if (this.state == state.ROUND4)
            {
                return "Round 4 State, next is Awards";
            }
            if (this.state == state.AWARDS)
            {
                return "Awards State, next is Go Home";
            }
            return "Unknown State";
        }

        private void buttonRoundSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.comboBoxJudges.SelectedItem == null)
            {
                Warn("Round Entry", "Judge required");
                return;
            }
            if (String.IsNullOrEmpty(this.txtJudgesScoreTeamA.Text) || String.IsNullOrEmpty(this.txtJudgesScoreTeamB.Text))
            {
                Warn("Round Entry", "Score required");
                return;
            }
            if (Convert.ToInt64(this.txtJudgesScoreTeamA.Text) < 28 || Convert.ToInt64(this.txtJudgesScoreTeamB.Text) < 28)
            {
                Warn("Round Entry", "Score too low to be valid");
                return;
            }
            pretty_student witness1 = (pretty_student)this.comboBoxWitness1.SelectedItem;
            pretty_student witness2 = (pretty_student)this.comboBoxWitness2.SelectedItem;
            pretty_student witness3 = (pretty_student)this.comboBoxWitness3.SelectedItem;
            pretty_student witness4 = (pretty_student)this.comboBoxWitness4.SelectedItem;
            pretty_student attorney1 = (pretty_student)this.comboBoxAttorney1.SelectedItem;
            pretty_student attorney2 = (pretty_student)this.comboBoxAttorney2.SelectedItem;
            pretty_student attorney3 = (pretty_student)this.comboBoxAttorney3.SelectedItem;
            pretty_student attorney4 = (pretty_student)this.comboBoxAttorney4.SelectedItem;
            List<pretty_student> students = new List<pretty_student>();
            try
            {
                students.Add(witness1);
                students.Add(witness2);
                students.Add(witness3);
                students.Add(witness4);
                students.Add(attorney1);
                students.Add(attorney2);
                students.Add(attorney3);
                students.Add(attorney4);
            }
            catch
            {
                Warn("Round Entry", "All ranks must be filled out");
                return;
            }
            foreach (pretty_student student in students)
            {
                int count = students.Count(stu => stu == student);
                if (count > 1)
                {
                    Warn("Round Entry", String.Format("Student: {0} used more than once", student.pretty));
                    return;
                }
            }
            Int64 round_number = 0;
            if (this.state == state.ROUND1)
            {
                round_number = 1;
            }
            else if (this.state == state.ROUND2)
            {
                round_number = 2;
            }
            else if (this.state == state.ROUND3)
            {
                round_number = 3;
            }
            else if (this.state == state.ROUND4)
            {
                round_number = 4;
            }
            else
            {
                Warn("Round Entry", "No valid rounds are in the works");
                return;
            }
            this.database.InsertRank(witness1.student.id, witness2.student.id, witness3.student.id, witness4.student.id, attorney1.student.id, attorney2.student.id, attorney3.student.id, attorney4.student.id);
            Int64 rank_id = this.database.GetRankID(witness1.student.id, witness2.student.id, witness3.student.id, witness4.student.id, attorney1.student.id, attorney2.student.id, attorney3.student.id, attorney4.student.id);
            
            var pretty_round = (pretty_round)this.comboBoxPairing.SelectedItem;
            var judge = (judge)this.comboBoxJudges.SelectedItem;
            if (pretty_round.judge_a.id == judge.id)
            {
                this.database.AddRankID(pretty_round.round, rank_id, judge_value.A);
                this.database.AddScore(pretty_round.round, Convert.ToInt64(this.txtJudgesScoreTeamA.Text), Convert.ToInt64(this.txtJudgesScoreTeamB.Text), judge_value.A);
            }
            else
            {
                this.database.AddRankID(pretty_round.round, rank_id, judge_value.B);
                this.database.AddScore(pretty_round.round, Convert.ToInt64(this.txtJudgesScoreTeamA.Text), Convert.ToInt64(this.txtJudgesScoreTeamB.Text), judge_value.B);
            }
            if (database.AwardsReady())
            {
                state = state.AWARDS;
            }
            Success("Round Entry", "Added witness ranks");
        }
        
        public enum pairing_state
        {
            FINE,
            START_OVER
        }

        private pairing_state FindJudges(List<judge> judges, Int64 number_of_one_judge_rounds, Int64 number_of_two_judge_rounds, Int64 number_of_three_judge_rounds, team team_a, team team_b, out judge judge_a, out judge judge_b, out judge judge_c, Int64 max_iterations)
        {
            List<judge> shuffled_judges = Shuffle<judge>(judges);
            var judges_stack = new Stack<judge>(shuffled_judges);
            judge_a = new judge();
            judge_b = null;
            judge_c = null;
            Int64 iterations = 0;
            while (true)
            {
                judge_a = judges_stack.Peek();
                if (judge_a.known_conflict1 == team_a.id || judge_a.known_conflict1 == team_b.id
                    || judge_a.known_conflict2 == team_a.id || judge_a.known_conflict2 == team_b.id
                    || judge_a.known_conflict3 == team_a.id || judge_a.known_conflict3 == team_b.id
                    || judge_a.known_conflict4 == team_a.id || judge_a.known_conflict4 == team_b.id
                    || judge_a.known_conflict5 == team_a.id || judge_a.known_conflict5 == team_b.id
                    || judge_a.known_conflict6 == team_a.id || judge_a.known_conflict6 == team_b.id
                    || judge_a.known_conflict7 == team_a.id || judge_a.known_conflict7 == team_b.id
                    || judge_a.known_conflict8 == team_a.id || judge_a.known_conflict8 == team_b.id
                    || judge_a.known_conflict9 == team_a.id || judge_a.known_conflict9 == team_b.id
                    || judge_a.known_conflict10 == team_a.id || judge_a.known_conflict10 == team_b.id)
                {
                    List<judge> temp_judges = judges_stack.ToList();
                    var temp_shuffled_judges = Shuffle(temp_judges);
                    judges_stack = new Stack<judge>(temp_shuffled_judges);
                    iterations += 1;
                    if (iterations > max_iterations)
                    {
                        return pairing_state.START_OVER;
                    }
                    continue;
                }
                judges_stack.Pop();
                break;
            }
            if (number_of_two_judge_rounds > 0 || number_of_three_judge_rounds > 0)
            {
                while (true)
                {
                    judge_b = judges_stack.Peek();
                    if (judge_b.known_conflict1 == team_a.id || judge_b.known_conflict1 == team_b.id
                        || judge_b.known_conflict2 == team_a.id || judge_b.known_conflict2 == team_b.id
                        || judge_b.known_conflict3 == team_a.id || judge_b.known_conflict3 == team_b.id
                        || judge_b.known_conflict4 == team_a.id || judge_b.known_conflict4 == team_b.id
                        || judge_b.known_conflict5 == team_a.id || judge_b.known_conflict5 == team_b.id
                        || judge_b.known_conflict6 == team_a.id || judge_b.known_conflict6 == team_b.id
                        || judge_b.known_conflict7 == team_a.id || judge_b.known_conflict7 == team_b.id
                        || judge_b.known_conflict8 == team_a.id || judge_b.known_conflict8 == team_b.id
                        || judge_b.known_conflict9 == team_a.id || judge_b.known_conflict9 == team_b.id
                        || judge_b.known_conflict10 == team_a.id || judge_b.known_conflict10 == team_b.id)
                    {
                        List<judge> temp_judges = judges_stack.ToList();
                        var temp_shuffled_judges = Shuffle(temp_judges);
                        judges_stack = new Stack<judge>(temp_shuffled_judges);
                        iterations += 1;
                        if (iterations > max_iterations)
                        {
                            return pairing_state.START_OVER;
                        }
                        continue;
                    }
                    judges_stack.Pop();
                    break;
                }
            }
            if (number_of_three_judge_rounds > 0)
            {
                while (true)
                {
                    judge_c = judges_stack.Peek();
                    if (judge_c.known_conflict1 == team_a.id || judge_c.known_conflict1 == team_b.id
                        || judge_c.known_conflict2 == team_a.id || judge_c.known_conflict2 == team_b.id
                        || judge_c.known_conflict3 == team_a.id || judge_c.known_conflict3 == team_b.id
                        || judge_c.known_conflict4 == team_a.id || judge_c.known_conflict4 == team_b.id
                        || judge_c.known_conflict5 == team_a.id || judge_c.known_conflict5 == team_b.id
                        || judge_c.known_conflict6 == team_a.id || judge_c.known_conflict6 == team_b.id
                        || judge_c.known_conflict7 == team_a.id || judge_c.known_conflict7 == team_b.id
                        || judge_c.known_conflict8 == team_a.id || judge_c.known_conflict8 == team_b.id
                        || judge_c.known_conflict9 == team_a.id || judge_c.known_conflict9 == team_b.id
                        || judge_c.known_conflict10 == team_a.id || judge_c.known_conflict10 == team_b.id)
                    {
                        List<judge> temp_judges = judges_stack.ToList();
                        var temp_shuffled_judges = Shuffle(temp_judges);
                        judges_stack = new Stack<judge>(temp_shuffled_judges);
                        iterations += 1;
                        if (iterations > max_iterations)
                        {
                            return pairing_state.START_OVER;
                        }
                        continue;
                    }
                    judges_stack.Pop();
                    break;
                }
            }
            return pairing_state.FINE;
        }

        private List<round> Round1Pairings(List<team> teams, List<judge> input_judges)
        {
            List<round> pairings = new List<round>();
            pairings = new List<round>();
            List<team> shuffled_teams;
            pairing_state state = pairing_state.START_OVER;
            Int64 number_of_pairs = teams.Count / 2;
            Int64 number_of_one_judge_rounds = number_of_pairs;
            Int64 number_of_two_judge_rounds = 0;
            Int64 number_of_three_judge_rounds = 0;
            pairings = new List<round>();
            if (input_judges.Count > number_of_pairs)
            {
                if (input_judges.Count > number_of_pairs * 2)
                {
                    //We can have three judge rounds
                    number_of_one_judge_rounds = 0;
                    if (input_judges.Count >= number_of_pairs * 3)
                    {
                        number_of_three_judge_rounds = number_of_pairs;
                    }
                    else
                    {
                        number_of_three_judge_rounds = (input_judges.Count % (number_of_pairs * 2));
                        number_of_two_judge_rounds = number_of_pairs - number_of_three_judge_rounds;
                    }
                }
                else
                {
                    //We can have two judge rounds
                    if (input_judges.Count == number_of_pairs * 2)
                    {
                        number_of_one_judge_rounds = 0;
                        number_of_two_judge_rounds = number_of_pairs;
                    }
                    else
                    {
                        number_of_two_judge_rounds = (input_judges.Count % number_of_pairs);
                        number_of_one_judge_rounds = number_of_pairs - number_of_two_judge_rounds;
                    }
                }
            }
            Int64 max_iterations = 1000;
            Int64 judges_man = 0;
            while (state == pairing_state.START_OVER)
            {
                if (judges_man > 1000)
                {
                    if (number_of_three_judge_rounds > 0)
                    {
                        number_of_three_judge_rounds -= 1;
                        number_of_two_judge_rounds += 1;
                    }
                    else if (number_of_two_judge_rounds > 0)
                    {
                        number_of_two_judge_rounds -= 1;
                        number_of_one_judge_rounds += 1;
                    }
                    judges_man = 0;
                }
                pairings = new List<round>();
                List<judge> judges = database.GetJudges();
                shuffled_teams = Shuffle(teams);
                var team_stack = new Stack<team>(teams);
                Int64 iterations = 0;
                team team_a = new team();
                team team_b = new team();
                while (team_stack.Count > 0)
                {
                    team_a = team_stack.Pop();
                    bool team_b_conflict = true;
                    while (team_b_conflict)
                    {
                        team_b = team_stack.Peek();
                        if (team_b.id == team_a.known_conflict1 || team_b.id == team_a.known_conflict2 || team_b.id == team_a.known_conflict3 || team_b.id == team_a.known_conflict4 || team_b.id == team_a.known_conflict5 || team_b.id == team_a.known_conflict6 || team_a.id == team_b.known_conflict1 || team_a.id == team_b.known_conflict2 || team_a.id == team_b.known_conflict4 || team_a.id == team_b.known_conflict5 || team_a.id == team_b.known_conflict6)
                        {
                            List<team> stacked_teams = team_stack.ToList();
                            var shuffled_stack_teams = Shuffle<team>(stacked_teams);
                            team_stack = new Stack<team>(shuffled_stack_teams);
                            iterations += 1;
                            if (iterations > max_iterations)
                            {
                                break;
                            }
                            continue;
                        }
                        break;
                    }
                    if (iterations > max_iterations)
                    {
                        break;
                    }
                    team_stack.Pop();
                    judge judge_a;
                    judge judge_b;
                    judge judge_c;
                    state = FindJudges(judges, number_of_one_judge_rounds, number_of_two_judge_rounds, number_of_three_judge_rounds, team_a, team_b, out judge_a, out judge_b, out judge_c, max_iterations);
                    judges.Remove(judge_a);
                    if (judge_b != null)
                    {
                        judges.Remove(judge_b);
                    }
                    if (judge_c != null)
                    {
                        judges.Remove(judge_c);
                    }
                    if (state == pairing_state.START_OVER)
                    {
                        if (state == pairing_state.START_OVER)
                        {
                            judges_man += 1;
                        }
                        continue;
                    }
                    Int64 judge_a_id = judge_a.id;
                    Int64 judge_b_id = 0;
                    Int64 judge_c_id = 0;
                    if (judge_b != null)
                    {
                        judge_b_id = judge_b.id;
                    }
                    if (judge_c != null)
                    {
                        judge_c_id = judge_c.id;
                    }
                    pairings.Add(new round { judge_a = judge_a_id, judge_b = judge_b_id, judge_c = judge_c_id, team_a = team_a.id, team_b = team_b.id, round_number = 1 });
                    if (judge_b != null)
                    {
                        if (judge_c != null)
                        {
                            number_of_three_judge_rounds -= 1;
                        }
                        else
                        {
                            number_of_two_judge_rounds -= 1;
                        }
                    }
                    else
                    {
                        number_of_one_judge_rounds -= 1;
                    }
                }
                if (iterations > max_iterations)
                {
                    continue;
                }
                
            }
            return pairings;
        }

        private List<round> Round2Pairings(List<team> teams, List<judge> input_judges)
        {
            List<score> scores = database.GetScores(2, teams);
            List<score> was_a = new List<score>();
            List<score> was_b = new List<score>();
            List<score> new_a = new List<score>();
            List<score> new_b = new List<score>();
            List<round> pairings = new List<round>();
            Int64 number_of_pairs = teams.Count / 2;
            Int64 number_of_one_judge_rounds = number_of_pairs;
            Int64 number_of_two_judge_rounds = 0;
            Int64 number_of_three_judge_rounds = 0;
            if (input_judges.Count > number_of_pairs)
            {
                if (input_judges.Count > number_of_pairs * 2)
                {
                    //We can have three judge rounds
                    number_of_one_judge_rounds = 0;
                    if (input_judges.Count >= number_of_pairs * 3)
                    {
                        number_of_three_judge_rounds = number_of_pairs;
                    }
                    else
                    {
                        number_of_three_judge_rounds = (input_judges.Count % (number_of_pairs * 2));
                        number_of_two_judge_rounds = number_of_pairs - number_of_three_judge_rounds;
                    }
                }
                else
                {
                    //We can have two judge rounds
                    if (input_judges.Count == number_of_pairs * 2)
                    {
                        number_of_one_judge_rounds = 0;
                        number_of_two_judge_rounds = number_of_pairs;
                    }
                    else
                    {
                        number_of_two_judge_rounds = (input_judges.Count % number_of_pairs);
                        number_of_one_judge_rounds = number_of_pairs - number_of_two_judge_rounds;
                    }
                }
            }
            for (int i = 0; i < scores.Count; ++i)
            {
                if (scores[i].last_side == last_side.A)
                {
                    was_a.Add(scores[i]);
                }
                else
                {
                    was_b.Add(scores[i]);
                }
            }
            if (was_a.Count != was_b.Count)
            {
                Exception ex = new Exception("Somehow the number of plaintiff and defense teams don't match up");
                throw ex;
            }
            bool final_no_conflicts = false;
            Int64 judges_man = 0;
            while (!final_no_conflicts)
            {
                if (judges_man > 1000)
                {
                    if (number_of_three_judge_rounds > 0)
                    {
                        number_of_three_judge_rounds -= 1;
                        number_of_two_judge_rounds += 1;
                    }
                    else if (number_of_two_judge_rounds > 0)
                    {
                        number_of_two_judge_rounds -= 1;
                        number_of_one_judge_rounds += 1;
                    }
                    judges_man = 0;
                }
                pairings = new List<round>();
                List<judge> judges = database.GetJudges();
                for (int i = 0; i < was_b.Count; i++)
                {
                    bool conflict = true;
                    Int64 conflict_count = 0;
                    while (conflict)
                    {
                        if (was_a[i].team.id == was_b[i].team.known_conflict1
                            || was_a[i].team.id == was_b[i].team.known_conflict2
                            || was_a[i].team.id == was_b[i].team.known_conflict3
                            || was_a[i].team.id == was_b[i].team.known_conflict4
                            || was_a[i].team.id == was_b[i].team.known_conflict5
                            || was_a[i].team.id == was_b[i].team.known_conflict6
                            || was_b[i].team.id == was_a[i].team.known_conflict1
                            || was_b[i].team.id == was_a[i].team.known_conflict2
                            || was_b[i].team.id == was_a[i].team.known_conflict3
                            || was_b[i].team.id == was_a[i].team.known_conflict4
                            || was_b[i].team.id == was_a[i].team.known_conflict5
                            || was_b[i].team.id == was_a[i].team.known_conflict6)
                        {
                            conflict_count += 1;
                            Int64 new_a_index = GetChangeIndex(was_a.Count, conflict_count, i);
                            var temp_a = was_a[i];
                            was_a[i] = was_a[(int)new_a_index];
                            was_a[(int)new_a_index] = temp_a;
                            continue;
                        }
                        break;
                    }
                }
                final_no_conflicts = true;
                for (int i = 0; i < was_b.Count; i++)
                {
                    if (was_a[i].team.id == was_b[i].team.known_conflict1
                            || was_a[i].team.id == was_b[i].team.known_conflict2
                            || was_a[i].team.id == was_b[i].team.known_conflict3
                            || was_a[i].team.id == was_b[i].team.known_conflict4
                            || was_a[i].team.id == was_b[i].team.known_conflict5
                            || was_a[i].team.id == was_b[i].team.known_conflict6
                            || was_b[i].team.id == was_a[i].team.known_conflict1
                            || was_b[i].team.id == was_a[i].team.known_conflict2
                            || was_b[i].team.id == was_a[i].team.known_conflict3
                            || was_b[i].team.id == was_a[i].team.known_conflict4
                            || was_b[i].team.id == was_a[i].team.known_conflict5
                            || was_b[i].team.id == was_a[i].team.known_conflict6)
                    {
                        final_no_conflicts = false;
                    }
                }
                if (!final_no_conflicts)
                {
                    continue;
                }
                Int64 max_iterations = 100;
                pairing_state state = pairing_state.START_OVER;
                for (int i = 0; i < was_b.Count; i++)
                {
                    judge judge_a;
                    judge judge_b;
                    judge judge_c;
                    state = FindJudges(judges, number_of_one_judge_rounds, number_of_two_judge_rounds, number_of_three_judge_rounds, was_b[i].team, was_a[i].team, out judge_a, out judge_b, out judge_c, max_iterations);
                    judges.Remove(judge_a);
                    if (judge_b != null)
                    {
                        judges.Remove(judge_b);
                    }
                    if (judge_c != null)
                    {
                        judges.Remove(judge_c);
                    }
                    if (state == pairing_state.START_OVER)
                    {
                        break;
                    }
                    Int64 judge_a_id = judge_a.id;
                    Int64 judge_b_id = 0;
                    Int64 judge_c_id = 0;
                    if (judge_b != null)
                    {
                        judge_b_id = judge_b.id;
                    }
                    if (judge_c != null)
                    {
                        judge_c_id = judge_c.id;
                    }
                    pairings.Add(new round { judge_a = judge_a_id, judge_b = judge_b_id, judge_c = judge_c_id, team_a = was_b[i].team.id, team_b = was_a[i].team.id, round_number = 1 });
                    if (judge_b != null)
                    {
                        if (judge_c != null)
                        {
                            number_of_three_judge_rounds -= 1;
                        }
                        else
                        {
                            number_of_two_judge_rounds -= 1;
                        }
                    }
                    else
                    {
                        number_of_one_judge_rounds -= 1;
                    }

                }
                if (state == pairing_state.START_OVER)
                {
                    judges_man += 1;
                }
                if (state == pairing_state.START_OVER || pairings.Count != teams.Count /2 )
                {
                    //Warn("pairings", "almost done but redo");
                    final_no_conflicts=false;
                    continue;
                }
                break;
            }
            return pairings;
        }

        private Int64 GetChangeIndex(Int64 number_of_items, Int64 conflict_count, Int64 current_index)
        {
            if (conflict_count + current_index > number_of_items - 1)
            {
                return current_index - conflict_count -1;
            }
            return current_index + conflict_count;
        }

        private List<round> Round3Pairings(List<team> teams, List<judge> input_judges)
        {
            List<score> scores = database.GetScores(3, teams);
            List<score> was_a = new List<score>();
            List<score> was_b = new List<score>();
            List<score> new_a = new List<score>();
            List<score> new_b = new List<score>();
            List<round> pairings = new List<round>();
            Random random = new Random();
            bool side_swap = random.NextDouble() > 0.5;
            Int64 number_of_pairs = teams.Count / 2;
            Int64 number_of_one_judge_rounds = number_of_pairs;
            Int64 number_of_two_judge_rounds = 0;
            Int64 number_of_three_judge_rounds = 0;
            if (input_judges.Count > number_of_pairs)
            {
                if (input_judges.Count > number_of_pairs * 2)
                {
                    //We can have three judge rounds
                    number_of_one_judge_rounds = 0;
                    if (input_judges.Count >= number_of_pairs * 3)
                    {
                        number_of_three_judge_rounds = number_of_pairs;
                    }
                    else
                    {
                        number_of_three_judge_rounds = (input_judges.Count % (number_of_pairs * 2));
                        number_of_two_judge_rounds = number_of_pairs - number_of_three_judge_rounds;
                    }
                }
                else
                {
                    //We can have two judge rounds
                    if (input_judges.Count == number_of_pairs * 2)
                    {
                        number_of_one_judge_rounds = 0;
                        number_of_two_judge_rounds = number_of_pairs;
                    }
                    else
                    {
                        number_of_two_judge_rounds = (input_judges.Count % number_of_pairs);
                        number_of_one_judge_rounds = number_of_pairs - number_of_two_judge_rounds;
                    }
                }
            }
            for (int i = 0; i < scores.Count; i += 2)
            {
                if (side_swap)
                {
                    was_a.Add(scores[i]);
                    was_b.Add(scores[i + 1]);
                }
                else
                {
                    was_b.Add(scores[i]);
                    was_a.Add(scores[i + 1]);
                }
            }
            if (was_a.Count != was_b.Count)
            {
                Exception ex = new Exception("Somehow the number of plaintiff and defense teams don't match up");
                throw ex;
            }
            bool final_no_conflicts = false;
            Int64 judges_man = 0;
            while (!final_no_conflicts)
            {
                if (judges_man > 1000)
                {
                    if (number_of_three_judge_rounds > 0)
                    {
                        number_of_three_judge_rounds -= 1;
                        number_of_two_judge_rounds += 1;
                    }
                    else if (number_of_two_judge_rounds > 0)
                    {
                        number_of_two_judge_rounds -= 1;
                        number_of_one_judge_rounds += 1;
                    }
                    judges_man = 0;
                }
                pairings = new List<round>();
                List<judge> judges = database.GetJudges();
                for (int i = 0; i < was_b.Count; i++)
                {
                    bool conflict = true;
                    Int64 conflict_count = 0;
                    while (conflict)
                    {
                        if (was_a[i].team.id == was_b[i].team.known_conflict1
                            || was_a[i].team.id == was_b[i].team.known_conflict2
                            || was_a[i].team.id == was_b[i].team.known_conflict3
                            || was_a[i].team.id == was_b[i].team.known_conflict4
                            || was_a[i].team.id == was_b[i].team.known_conflict5
                            || was_a[i].team.id == was_b[i].team.known_conflict6
                            || was_b[i].team.id == was_a[i].team.known_conflict1
                            || was_b[i].team.id == was_a[i].team.known_conflict2
                            || was_b[i].team.id == was_a[i].team.known_conflict3
                            || was_b[i].team.id == was_a[i].team.known_conflict4
                            || was_b[i].team.id == was_a[i].team.known_conflict5
                            || was_b[i].team.id == was_a[i].team.known_conflict6)
                        {
                            conflict_count += 1;
                            Int64 new_a_index = GetChangeIndex(was_a.Count, conflict_count, i);
                            var temp_a = was_a[i];
                            was_a[i] = was_a[(int)new_a_index];
                            was_a[(int)new_a_index] = temp_a;
                            continue;
                        }
                        break;
                    }
                }
                final_no_conflicts = true;
                for (int i = 0; i < was_b.Count; i++)
                {
                    if (was_a[i].team.id == was_b[i].team.known_conflict1
                            || was_a[i].team.id == was_b[i].team.known_conflict2
                            || was_a[i].team.id == was_b[i].team.known_conflict3
                            || was_a[i].team.id == was_b[i].team.known_conflict4
                            || was_a[i].team.id == was_b[i].team.known_conflict5
                            || was_a[i].team.id == was_b[i].team.known_conflict6
                            || was_b[i].team.id == was_a[i].team.known_conflict1
                            || was_b[i].team.id == was_a[i].team.known_conflict2
                            || was_b[i].team.id == was_a[i].team.known_conflict3
                            || was_b[i].team.id == was_a[i].team.known_conflict4
                            || was_b[i].team.id == was_a[i].team.known_conflict5
                            || was_b[i].team.id == was_a[i].team.known_conflict6)
                    {
                        final_no_conflicts = false;
                    }
                }
                if (!final_no_conflicts)
                {
                    continue;
                }
                Int64 max_iterations = 100;
                pairing_state state = pairing_state.START_OVER;
                for (int i = 0; i < was_b.Count; i++)
                {
                    judge judge_a;
                    judge judge_b;
                    judge judge_c;
                    state = FindJudges(judges, number_of_one_judge_rounds, number_of_two_judge_rounds, number_of_three_judge_rounds, was_b[i].team, was_a[i].team, out judge_a, out judge_b, out judge_c, max_iterations);
                    judges.Remove(judge_a);
                    if (judge_b != null)
                    {
                        judges.Remove(judge_b);
                    }
                    if (judge_c != null)
                    {
                        judges.Remove(judge_c);
                    }
                    if (state == pairing_state.START_OVER)
                    {
                        break;
                    }
                    Int64 judge_a_id = judge_a.id;
                    Int64 judge_b_id = 0;
                    Int64 judge_c_id = 0;
                    if (judge_b != null)
                    {
                        judge_b_id = judge_b.id;
                    }
                    if (judge_c != null)
                    {
                        judge_c_id = judge_c.id;
                    }
                    pairings.Add(new round { judge_a = judge_a_id, judge_b = judge_b_id, judge_c = judge_c_id, team_a = was_b[i].team.id, team_b = was_a[i].team.id, round_number = 1 });
                    if (judge_b != null)
                    {
                        if (judge_c != null)
                        {
                            number_of_three_judge_rounds -= 1;
                        }
                        else
                        {
                            number_of_two_judge_rounds -= 1;
                        }
                    }
                    else
                    {
                        number_of_one_judge_rounds -= 1;
                    }

                }
                if (state == pairing_state.START_OVER)
                {
                    judges_man += 1;
                }
                if (state == pairing_state.START_OVER || pairings.Count != teams.Count / 2)
                {
                    //Warn("pairings", "almost done but redo");
                    final_no_conflicts = false;
                    continue;
                }
                break;
            }
            return pairings;
        }

        private List<round> Round4Pairings(List<team> teams, List<judge> input_judges)
        {
            List<score> scores = database.GetScores(3, teams);
            List<score> was_a = new List<score>();
            List<score> was_b = new List<score>();
            List<score> new_a = new List<score>();
            List<score> new_b = new List<score>();
            List<round> pairings = new List<round>();
            Int64 number_of_pairs = teams.Count / 2;
            Int64 number_of_one_judge_rounds = number_of_pairs;
            Int64 number_of_two_judge_rounds = 0;
            Int64 number_of_three_judge_rounds = 0;
            if (input_judges.Count > number_of_pairs)
            {
                if (input_judges.Count > number_of_pairs * 2)
                {
                    //We can have three judge rounds
                    number_of_one_judge_rounds = 0;
                    if (input_judges.Count >= number_of_pairs * 3)
                    {
                        number_of_three_judge_rounds = number_of_pairs;
                    }
                    else
                    {
                        number_of_three_judge_rounds = (input_judges.Count % (number_of_pairs * 2));
                        number_of_two_judge_rounds = number_of_pairs - number_of_three_judge_rounds;
                    }
                }
                else
                {
                    //We can have two judge rounds
                    if (input_judges.Count == number_of_pairs * 2)
                    {
                        number_of_one_judge_rounds = 0;
                        number_of_two_judge_rounds = number_of_pairs;
                    }
                    else
                    {
                        number_of_two_judge_rounds = (input_judges.Count % number_of_pairs);
                        number_of_one_judge_rounds = number_of_pairs - number_of_two_judge_rounds;
                    }
                }
            }
            for (int i = 0; i < scores.Count; ++i)
            {
                if (scores[i].last_side == last_side.A)
                {
                    was_a.Add(scores[i]);
                }
                else
                {
                    was_b.Add(scores[i]);
                }
            }
            if (was_a.Count != was_b.Count)
            {
                Exception ex = new Exception("Somehow the number of plaintiff and defense teams don't match up");
                throw ex;
            }
            bool final_no_conflicts = false;
            Int64 judges_man = 0;
            while (!final_no_conflicts)
            {
                if (judges_man > 1000)
                {
                    if (number_of_three_judge_rounds > 0)
                    {
                        number_of_three_judge_rounds -= 1;
                        number_of_two_judge_rounds += 1;
                    }
                    else if (number_of_two_judge_rounds > 0)
                    {
                        number_of_two_judge_rounds -= 1;
                        number_of_one_judge_rounds += 1;
                    }
                    judges_man = 0;
                }
                pairings = new List<round>();
                List<judge> judges = database.GetJudges();
                for (int i = 0; i < was_b.Count; i++)
                {
                    bool conflict = true;
                    Int64 conflict_count = 0;
                    while (conflict)
                    {
                        if (was_a[i].team.id == was_b[i].team.known_conflict1
                            || was_a[i].team.id == was_b[i].team.known_conflict2
                            || was_a[i].team.id == was_b[i].team.known_conflict3
                            || was_a[i].team.id == was_b[i].team.known_conflict4
                            || was_a[i].team.id == was_b[i].team.known_conflict5
                            || was_a[i].team.id == was_b[i].team.known_conflict6
                            || was_b[i].team.id == was_a[i].team.known_conflict1
                            || was_b[i].team.id == was_a[i].team.known_conflict2
                            || was_b[i].team.id == was_a[i].team.known_conflict3
                            || was_b[i].team.id == was_a[i].team.known_conflict4
                            || was_b[i].team.id == was_a[i].team.known_conflict5
                            || was_b[i].team.id == was_a[i].team.known_conflict6)
                        {
                            conflict_count += 1;
                            Int64 new_a_index = GetChangeIndex(was_a.Count, conflict_count, i);
                            var temp_a = was_a[i];
                            was_a[i] = was_a[(int)new_a_index];
                            was_a[(int)new_a_index] = temp_a;
                            continue;
                        }
                        break;
                    }
                }
                final_no_conflicts = true;
                for (int i = 0; i < was_b.Count; i++)
                {
                    if (was_a[i].team.id == was_b[i].team.known_conflict1
                            || was_a[i].team.id == was_b[i].team.known_conflict2
                            || was_a[i].team.id == was_b[i].team.known_conflict3
                            || was_a[i].team.id == was_b[i].team.known_conflict4
                            || was_a[i].team.id == was_b[i].team.known_conflict5
                            || was_a[i].team.id == was_b[i].team.known_conflict6
                            || was_b[i].team.id == was_a[i].team.known_conflict1
                            || was_b[i].team.id == was_a[i].team.known_conflict2
                            || was_b[i].team.id == was_a[i].team.known_conflict3
                            || was_b[i].team.id == was_a[i].team.known_conflict4
                            || was_b[i].team.id == was_a[i].team.known_conflict5
                            || was_b[i].team.id == was_a[i].team.known_conflict6)
                    {
                        final_no_conflicts = false;
                    }
                }
                if (!final_no_conflicts)
                {
                    continue;
                }
                Int64 max_iterations = 100;
                pairing_state state = pairing_state.START_OVER;
                for (int i = 0; i < was_b.Count; i++)
                {
                    judge judge_a;
                    judge judge_b;
                    judge judge_c;
                    state = FindJudges(judges, number_of_one_judge_rounds, number_of_two_judge_rounds, number_of_three_judge_rounds, was_b[i].team, was_a[i].team, out judge_a, out judge_b, out judge_c, max_iterations);
                    judges.Remove(judge_a);
                    if (judge_b != null)
                    {
                        judges.Remove(judge_b);
                    }
                    if (judge_c != null)
                    {
                        judges.Remove(judge_c);
                    }
                    if (state == pairing_state.START_OVER)
                    {
                        break;
                    }
                    Int64 judge_a_id = judge_a.id;
                    Int64 judge_b_id = 0;
                    Int64 judge_c_id = 0;
                    if (judge_b != null)
                    {
                        judge_b_id = judge_b.id;
                    }
                    if (judge_c != null)
                    {
                        judge_c_id = judge_c.id;
                    }
                    pairings.Add(new round { judge_a = judge_a_id, judge_b = judge_b_id, judge_c = judge_c_id, team_a = was_b[i].team.id, team_b = was_a[i].team.id, round_number = 1 });
                    if (judge_b != null)
                    {
                        if (judge_c != null)
                        {
                            number_of_three_judge_rounds -= 1;
                        }
                        else
                        {
                            number_of_two_judge_rounds -= 1;
                        }
                    }
                    else
                    {
                        number_of_one_judge_rounds -= 1;
                    }

                }
                if (state == pairing_state.START_OVER)
                {
                    judges_man += 1;
                }
                if (state == pairing_state.START_OVER || pairings.Count != teams.Count / 2)
                {
                    final_no_conflicts = false;
                    continue;
                }
                break;
            }
            return pairings;
        }

        private void buttonGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (this.state == state.ROUND4 || this.state == state.AWARDS)
            {
                Warn("Pairings", "All round pairings are complete");
                return;
            }
            var teams = database.GetTeams();
            if (teams.Count % 2 != 0)
            {
                Warn("Pairings", "Need an even number of teams for pairings");
                return;
            }
            var judges = database.GetJudges();
            if (teams.Count / 2 > judges.Count)
            {
                Warn("Pairings", "Not enough judges to make pairings");
                return;
            }
            List<round> pairings;
            Int64 round_number = 0;
            if (this.state == state.PRELIMMINARY)
            {
                pairings = Round1Pairings(teams, judges);
                round_number = 1;
            }
            else if (this.state == state.ROUND1)
            {
                pairings = Round2Pairings(teams, judges);
                round_number = 2;
            }
            else if (this.state == state.ROUND2)
            {
                pairings = Round3Pairings(teams, judges);
                round_number = 3;
            }
            else if (this.state == state.ROUND3)
            {
                pairings = Round4Pairings(teams, judges);
                round_number = 4;
            }
            else
            {
                Warn("Pairings", "Unknown state reached, contact developer");
                return;
            }
            foreach(round pairing in pairings)
            {
                database.InsertRound(round_number: round_number, team_a: pairing.team_a, team_b: pairing.team_b, judge_a: pairing.judge_a, judge_b: pairing.judge_b, judge_c: pairing.judge_c);
                database.AddTeamConflict(pairing.team_a, pairing.team_b);
                database.AddTeamConflict(pairing.team_b, pairing.team_a);
                database.AddJudgeConflict(pairing.judge_a, pairing.team_a);
                database.AddJudgeConflict(pairing.judge_a, pairing.team_b);
                if (pairing.judge_b != 0)
                {
                    database.AddJudgeConflict(pairing.judge_b, pairing.team_a);
                    database.AddJudgeConflict(pairing.judge_b, pairing.team_b);
                }
                if (pairing.judge_c != 0)
                {
                    database.AddJudgeConflict(pairing.judge_c, pairing.team_a);
                    database.AddJudgeConflict(pairing.judge_c, pairing.team_b);
                }
            }
            List<pretty_round> pretty_pairings = new List<pretty_round>();
            foreach (round round in pairings)
            {
                pretty_pairings.Add(RoundToPrettyRound(round));
            }
            this.listViewRoundPairings.ItemsSource = pretty_pairings;
            this.state += 1;
            this.labelStateEnter.Content = stateToText();
            this.labelStateGenerate.Content = stateToText();
        }

        private void buttonStudentsSave_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(this.txtStudentName.Text))
            {
                Warn("Student Creation", "Student name cannot be empty");
                return;
            }
            if(this.radioButtonDefense1.IsChecked == this.radioButtonPlaintiff1.IsChecked)
            {
                Warn("Student Creation", "Plaintiff or Defense must be chosen");
                return;
            }
            if (this.comboBoxTeams.SelectedItem == null)
            {
                Warn("Student Creation", "A team must be chosen");
                return;
            }
            var team = (team)this.comboBoxTeams.SelectedItem;
            String side = ((bool)this.radioButtonDefense1.IsChecked) ? "Defense" : "Plaintiff";
            String name = this.txtStudentName.Text;
            database.InsertStudent(name, side, team.id);
            Success("Student Creation", name);
        }

        private void buttonJudgesSave_Click(object sender, RoutedEventArgs e)
        {
            Int64 known_conflict1 = 0;
            Int64 known_conflict2 = 0;
            Int64 known_conflict3 = 0;
            Int64 known_conflict4 = 0;
            Int64 known_conflict5 = 0;
            Int64 known_conflict6 = 0;
            Int64 known_conflict7 = 0;
            if (String.IsNullOrEmpty(this.txtJudgesName.Text))
            {
                Warn("Judge Creation", "Judge name is required");
                return;
            }
            var conflicts = this.listViewTeamsForJudges.SelectedItems;
            if (conflicts.Count > 0)
            {
                known_conflict1 = ((team)conflicts[0]).id;
            }
            if (conflicts.Count > 1)
            {
                known_conflict2 = ((team)conflicts[1]).id;
            }
            if (conflicts.Count > 2)
            {
                known_conflict3 = ((team)conflicts[2]).id;
            }
            if (conflicts.Count > 3)
            {
                known_conflict4 = ((team)conflicts[3]).id;
            }
            if (conflicts.Count > 4)
            {
                known_conflict5 = ((team)conflicts[4]).id;
            }
            if (conflicts.Count > 5)
            {
                known_conflict6 = ((team)conflicts[5]).id;
            }
            if (conflicts.Count > 6)
            {
                known_conflict7 = ((team)conflicts[6]).id;
            }
            if (conflicts.Count > 7)
            {
                Warn("Judge Creation", "Maximum conflicts is 7");
                return;
            }
            database.InsertJudge(this.txtJudgesName.Text, known_conflict1, known_conflict2, known_conflict3, known_conflict4, known_conflict5, known_conflict6, known_conflict7);
            Success("Judge Creation", this.txtJudgesName.Text);
        }

        private void buttonJudgesLoad_Click(object sender, RoutedEventArgs e)
        {
            List<team> teams = database.GetTeams();
            this.listViewTeamsForJudges.ItemsSource = teams;
        }

        private void buttonTeamsSave_Click(object sender, RoutedEventArgs e)
        {
            Int64 known_conflict1 = 0;
            Int64 known_conflict2 = 0;
            Int64 known_conflict3 = 0;
            Int64 team_number = 0;
            if (String.IsNullOrEmpty(this.txtTeamName.Text))
            {
                Warn("Team Creation", "Team name is required");
                return;
            }
            var conflicts = this.listViewTeamsForTeams.SelectedItems;
            if (conflicts.Count > 0)
            {
                known_conflict1 = ((team)conflicts[0]).id;
            }
            if (conflicts.Count > 1)
            {
                known_conflict2 = ((team)conflicts[1]).id;
            }
            if (conflicts.Count > 2)
            {
                known_conflict3 = ((team)conflicts[2]).id;
            }
            if (conflicts.Count > 3)
            {
                Warn("Team Creation", "Maximum conflict is 3");
                return;
            }
            String obfuscated_name;
            if (!String.IsNullOrEmpty(this.txtObfuscatedName.Text))
            {
                obfuscated_name = this.txtObfuscatedName.Text;
            }
            else
            {
                Warn("Team Creation", "Obfuscated name is required");
                return;
            }
            if (!String.IsNullOrEmpty(this.txtTeamNumber.Text))
            {
                team_number = Convert.ToInt64(this.txtTeamNumber.Text);
            }
            database.InsertTeam(this.txtTeamName.Text, obfuscated_name, team_number, known_conflict1, known_conflict2, known_conflict3);
            List<team> teams = database.GetTeams();
            Success("Team Creation", this.txtTeamName.Text);
            this.listViewTeamsForTeams.ItemsSource = teams;
        }

        private void buttonStudentsLoad_Click(object sender, RoutedEventArgs e)
        {
            List<team> teams = database.GetTeams();
            this.comboBoxTeams.ItemsSource = teams;
        }

        private void buttonLoadRoundEntry_Click(object sender, RoutedEventArgs e)
        {
            Int64 round_number = 0;
            if (this.state == state.ROUND1)
            {
                round_number = 1;
            }
            else if (this.state == state.ROUND2)
            {
                round_number = 2;
            }
            else if (this.state == state.ROUND3)
            {
                round_number = 3;
            }
            else if (this.state == state.ROUND4)
            {
                round_number = 4;
            }
            else
            {
                Warn("Round Entry", "No valid rounds are in the works");
                return;
            }
            var pairings = database.GetRounds(round_number);
            List<pretty_round> pretty_pairings = new List<pretty_round>();
            foreach (round round in pairings)
            {
                pretty_pairings.Add(RoundToPrettyRound(round));
            }
            this.comboBoxPairing.ItemsSource = pretty_pairings;
        }

        private void buttonLoadSpecificRoundEntry_Click(object sender, RoutedEventArgs e)
        {
            pretty_round pairing = (pretty_round)this.comboBoxPairing.SelectedItem;
            List<judge> judges = new List<judge>();
            judges.Add(pairing.judge_a);
            if (pairing.judge_b != null)
            {
                judges.Add(pairing.judge_b);
            }
            if (pairing.judge_c != null)
            {
                judges.Add(pairing.judge_c);
            }
            this.comboBoxJudges.ItemsSource = judges;
            List<student> team_a_students = database.GetStudentsFromTeamID(pairing.team_a.id);
            List<student> team_b_students = database.GetStudentsFromTeamID(pairing.team_b.id);
            List<pretty_student> all_students = new List<pretty_student>();
            foreach (student student in team_a_students)
            {
                all_students.Add(StudentToPrettyStudent(student));
            }
            foreach (student student in team_b_students)
            {
                all_students.Add(StudentToPrettyStudent(student));
            }
            this.comboBoxAttorney1.ItemsSource = all_students;
            this.comboBoxAttorney2.ItemsSource = all_students;
            this.comboBoxAttorney3.ItemsSource = all_students;
            this.comboBoxAttorney4.ItemsSource = all_students;
            this.comboBoxWitness1.ItemsSource = all_students;
            this.comboBoxWitness2.ItemsSource = all_students;
            this.comboBoxWitness3.ItemsSource = all_students;
            this.comboBoxWitness4.ItemsSource = all_students;
        }

        private void buttonTournamentWinnersLoad_Click(object sender, RoutedEventArgs e)
        {
            if (!database.AwardsReady())
            {
                Warn("Tournament Winners", "You cannot generate awards until all rounds are tabulated");
                return;
            }
            var teams = database.GetTeams();
            var scores = database.GetScores(4, teams);
            scores.Reverse();
            this.listViewTournamentWinners.ItemsSource = scores;
        }

        private void buttonPersonalWinners_Click(object sender, RoutedEventArgs e)
        {
            if (!database.AwardsReady())
            {
                Warn("Tournament Winners", "You cannot generate awards until all rounds are tabulated");
                return;
            }
            var students = database.GetStudents();
            var ranks = database.GetRanks();
            var rounds = database.GetRounds();
            Dictionary<student, Int64> individual_ranks = new Dictionary<student, Int64>();
            Dictionary<Int64, student> student_ids = new Dictionary<Int64, student>();
            Dictionary<Int64, rank> rank_ids = new Dictionary<Int64, rank>();
            foreach (student student in students)
            {
                student_ids[student.id] = student;
                individual_ranks[student] = 0;
            }
            foreach (rank rank in ranks)
            {
                rank_ids[rank.id] = rank;
            }
            foreach (round round in rounds)
            {
                rank judge_a_ranks = rank_ids[round.judge_a_ranks];
                if (round.judge_b == 0)
                {
                    individual_ranks[student_ids[judge_a_ranks.witness1]] += 10;
                    individual_ranks[student_ids[judge_a_ranks.attorney1]] += 10;
                    individual_ranks[student_ids[judge_a_ranks.witness2]] += 8;
                    individual_ranks[student_ids[judge_a_ranks.attorney2]] += 8;
                    individual_ranks[student_ids[judge_a_ranks.witness3]] += 6;
                    individual_ranks[student_ids[judge_a_ranks.attorney3]] += 6;
                    individual_ranks[student_ids[judge_a_ranks.witness4]] += 4;
                    individual_ranks[student_ids[judge_a_ranks.attorney4]] += 4;
                }
                else
                {
                    individual_ranks[student_ids[judge_a_ranks.witness1]] += 5;
                    individual_ranks[student_ids[judge_a_ranks.attorney1]] += 5;
                    individual_ranks[student_ids[judge_a_ranks.witness2]] += 4;
                    individual_ranks[student_ids[judge_a_ranks.attorney2]] += 4;
                    individual_ranks[student_ids[judge_a_ranks.witness3]] += 3;
                    individual_ranks[student_ids[judge_a_ranks.attorney3]] += 3;
                    individual_ranks[student_ids[judge_a_ranks.witness4]] += 2;
                    individual_ranks[student_ids[judge_a_ranks.attorney4]] += 2;
                    rank judge_b_ranks = rank_ids[round.judge_b_ranks];
                    individual_ranks[student_ids[judge_b_ranks.witness1]] += 5;
                    individual_ranks[student_ids[judge_b_ranks.attorney1]] += 5;
                    individual_ranks[student_ids[judge_b_ranks.witness2]] += 4;
                    individual_ranks[student_ids[judge_b_ranks.attorney2]] += 4;
                    individual_ranks[student_ids[judge_b_ranks.witness3]] += 3;
                    individual_ranks[student_ids[judge_b_ranks.attorney3]] += 3;
                    individual_ranks[student_ids[judge_b_ranks.witness4]] += 2;
                    individual_ranks[student_ids[judge_b_ranks.attorney4]] += 2;
                }
            }
            List<KeyValuePair<student, Int64>> rank_list = individual_ranks.ToList();
            rank_list.Sort(delegate (KeyValuePair<student, Int64> pair1, KeyValuePair<student, Int64> pair2) { return pair1.Value.CompareTo(pair2.Value); });
            List<pretty_student> student_ranks = new List<pretty_student>();
            foreach (var kv in rank_list)
            {
                pretty_student pretty_student = StudentToPrettyStudent(kv.Key);
                pretty_student.points = kv.Value;
                student_ranks.Add(pretty_student);
            }
            student_ranks.Reverse();
            this.listViewPersonalWinners.ItemsSource = student_ranks;
        }

        private void buttonViewRound_Click(object sender, RoutedEventArgs e)
        {
            Int64 round_number = Convert.ToInt64(this.comboBoxRoundNumber.SelectedItem);
            var rounds = database.GetRounds(round_number);
            List<pretty_round> pretty_rounds = new List<pretty_round>();
            foreach (round round in rounds)
            {
                pretty_rounds.Add(RoundToPrettyRound(round));
            }
            this.listViewSpecificRoundPairings.ItemsSource = pretty_rounds;
        }
    }
}
