using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cubing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CubeWorld cw;
        Cube c ;
        Cube3D c3d;
        int currentMoveID = 0;
        string[] moves = new string[0];
        bool autoMoving = false;
        Rotation currentRotation;

        public MainWindow()
        {
            InitializeComponent();
            cw = new CubeWorld(myViewport3D, this);
        }

        private void btnCreateByDimension_Click(object sender, RoutedEventArgs e)
        {

            c = new Cube(Convert.ToInt32(textboxDimension.Text));
            c3d = new Cube3D(c);
            cw.Cube3D = c3d;
            c3d.AnimationCompleted += C3d_AnimationCompleted;
            ShowWholeCube();
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            int dim = Convert.ToInt32(textboxDimension.Text);
            int depth = 0;
            if (textboxDepth.Text.Trim() == string.Empty)
            {
                depth = dim * dim * 3;
            }
            else
            {
                depth = Convert.ToInt32(textboxDepth.Text.Trim());
            }
            Scrambler s = new Scrambler(dim, depth);
            c = new Cube(dim);
            for (int i = 0; i < s.Moves.Length; i++)
            {
                c.Move(s.Moves[i]);
            }
            textboxCubeString.Text = c.CubeString;
            textboxMoves.Text = "";

            c = new Cube(textboxCubeString.Text.Trim());
            c3d = new Cube3D(c);
            cw.Cube3D = c3d;
            c3d.AnimationCompleted += C3d_AnimationCompleted;
            ShowWholeCube();

        }

        public delegate void UpdateSolutionText( string sol);
        private Cursor cursor;
        private void Cs_SolutionFound(CubeSolver sender, string sol)
        {
            sender.SolutionFound -= Cs_SolutionFound;
            textboxMoves.Dispatcher.Invoke(new UpdateSolutionText((s) => {
                textboxMoves.Text = s;
                this.Cursor = cursor;
                MessageBox.Show("Solution Found!");
            }), sol);
        }

        private void btnGetSolution_Click(object sender, RoutedEventArgs e)
        {
            CubeSolver cs = new CubeSolver();
            cs.SolutionFound += Cs_SolutionFound;
            cs.BeginFind(c.CubeString);
            cursor = this.Cursor;
            this.Cursor = Cursors.Wait;
        }

        private void btnDemo_Click(object sender, RoutedEventArgs e)
        {
            //for test
            string[] cs = new string[]
            {
                "DLRRFULLDUBFDURDBFBRBLFU",
                "RRBBUFBFBRLRRRFRDDURUBFBBRFLUDUDFLLFFLLLLDFBDDDUUBDLUU",
                "FLDFDLBDFBLFFRRBDRFRRURBRDUBBDLURUDRRBFFBDLUBLUULUFRRFBLDDUULBDBDFLDBLUBFRFUFBDDUBFLLRFLURDULLRU",
                "LLFBRBFUDULBULBBDDUBBBBLDFDULDLURFBDFRLDUFDBRLDUFBLURFRFRDRBULFBLLLBURUFRFURDDLBULLLRLRDFRDRBBRUDFDUFRBUDULFDUFULDFRBRBULLUFFBLRDDDDFRRBUBRLBUUFFRRDFF",
                "FBDDDFFUDRFBBLFLLURLDLLUFBLRFDUFLBLLFBFLRRBBFDRRDUBUFRBUBRDLUBFDRLBBRLRUFLBRBDUDFFFDBLUDBBLRDFUUDLBBBRRDRUDLBLDFRUDLLFFUUBFBUUFDLRUDUDBRRBBUFFDRRRDBULRRURULFDBRRULDDRUUULBLLFDFRRFDURFFLDUUBRUFDRFUBLDFULFBFDDUDLBLLRBL",
                "DBDBDDFBDDLUBDLFRFRBRLLDUFFDUFRBRDFDRUFDFDRDBDBULDBDBDBUFBUFFFULLFLDURRBBRRBRLFUUUDUURBRDUUURFFFLRFLRLDLBUFRLDLDFLLFBDFUFRFFUUUFURDRFULBRFURRBUDDRBDLLRLDLLDLUURFRFBUBURBRUDBDDLRBULBULUBDBBUDRBLFFBLRBURRUFULBRLFDUFDDBULBRLBUFULUDDLLDFRDRDBBFBUBBFLFFRRUFFRLRRDRULLLFRLFULBLLBBBLDFDBRBFDULLULRFDBR",
                "DRRRURBDDBFBRBDDBRRDUFLLURFBFLFURLFLFRBRFUBDRFDFUUBLFFFUULBBFDBDFBUBBFRFLRDLFDRBBLLFRLDFDRBURULDDRFFBFUUBLLFBRUUFDUBRDBBRDFLURUUFFUDLBRRFDUBFLRUUFFRLBFRFLRULUDFRUBBDBFFLBBDFDFLDBFRRRDDLFLBRBFBBRULDDUUBLBBURULLDDLDRUDRBUDRLUULDURLRDFLFULUFLFULRDDDUBBULRBRDFBBLFURRLULUBDDULRFBRFURBRLBRUBULBDDFBUFFBBRLRUUUFRULLBFFRFDDFFDULLDLBUDLLLLUUBBLDLLBBULULBDUDDFUBFLLDLDLFRDUDDBRRFRURRFRRLDDDDRD",
                "RFBLRUFLLFFLRRBDUDDBBBDUDFRUDUFFFBBFRBRDURBULFUDDFLLLDLFLRDLDBBBUUBRDBBBDFUFRUURULURBURDLFDUBFFDRDFRUBDUBRFLRRLUDLRLFBLBRRLLRDRBRBLURBLLRFRLDDFFFRBFUFURDFRRUDUFDDRRRLFLLUBBLBFDRRDLBRLUUBRDBBUBFLUUFBLLDBFFFBUFBFDBRDDDFLRFFBFFFLFRRDUUDDBUBLUUDURRBDBFFLFURDDLUBULUULULBFBRUBLLDDFLRBDBRFDUUDFURLLUBUFBLULLURDLLLBLFFRLLBLUDRLRDBLDDBRBUDRBLLRDUUUBRRFBFBBULUDUDLDRFUDDDFULRFRBDUDULBRRDBDFFRUUFRRFBDBLFBBDFURLRFDUUFRLUBURFURDDFLDFUBDFRRURRDLUDRBRBDLBFLBBRDLRDBFDUBDFFUBLFLUULLBUDLLLURDBLFFFDFLF",
                "ULBDLDBUFRBBBBBLBFFFDFRFBBDDFDFRFFLDLDLURRBUDRRBFLUDFRLBDURULRUUDBBBUBRURRRLDLRFFUFFFURRFBLLRRFLFUDBDRRDFULLLURFBFUUBDBBDBFLFDFUUFDUBRLUFDBLRFLUDUFBFDULDFRUBLBBBUBRRDBDDDDFURFLRDBRRLLRFUFLRDFDUULRRDULFDUDRFLBFRLDUDBDFLDBDUFULULLLBUUFDFFDBBBRBRLFLUFLFUFFRLLLFLBUDRRFDDUDLFLBRDULFLBLLULFLDLUULBUDRDFLUDDLLRBLUBBRFRRLDRDUUFLDDFUFLBDBBLBURBBRRRFUBLBRBRUBFFDBBBBLBUFBLURBLDRFLFBUDDFFRFFRLBDBDUURBUFBDFFFLFBDLDUFFBRDLBRLRLBFRUUUULRRBDBRRFDLLRRUUBDBDBFDLRDDBRUUUUUBLLURBDFUFLLRDBLRRBBLBDDBBFUDUDLDLUFDDDUURBFUFRRBLLURDDRURRURLBLDRFRUFBDRULUFFDUDLBBUURFDUDBLRRUDFRLLDULFUBFDLURFBFULFLRRRRRFDDDLFDDRUFRRLBLUBU",
                "URBDLBURUBUFDDRBUFDDUBFRDBLRFUUBLDRLBBUBLUFBLRBFFDDLLLDRULRBUDBBRLULBUUUFFRUBLUFUUBRLBLUUDRLDUBLUFDLBLFDBLFRLFLRUDFLLLDFULDRFDFLLFLLRDUUDRFRFFFFBFRBFRFUUBBUULURLULBBLLDDLBUFRURBLFFBRFFRRURFUFLLRUFDBDDDBLRDUBLBRBLRLBUBULLBBFRBFRBLFUDDULLFBRLDUFBBLDBBFBDBDUFFRRDBUUULDDBRFRDBDDRDRFUDBBFULDLUDBLRRFDBLDBLRFFBDLRBUBLDLUDFUFBDDRUDRDFUBLFFRBDFDDDBULBDRBUDBBDRLUFUFFFFDRBDLRLRDRRBFRLUBFFURFRLRLRRLDBLFLFULBURULDURURBLUFUULFBFULULUDLFLLUFLRFLRLFFDRBLLFRDDDBFBFFBLULDLRUDUURLRUFDRFRDURDUFFFULRRUURUDFURBDLBLLDFBBRDRLUBULFRDLRUFUFDBBDLRRURFULBBBBDBRUDUUULFFBUDLRUBRDDRBFDDBURRLURRDDRRRUURURFUBRRBFRFFFBLBLLDURBDRDFDFRRDRDDLDUFUFFLBDLBBDFLLLFDULDBLDRUUFDURBDLUBBLBDDFFDUURBLDRBRLDBLUDRLRFBFDUFUUBFBRDRFFFFUDDDRFFLDFLRRBLDRRDRFBFBLUDBFBBB",
                "FBDRLBLRRURRLDRBDLBURDFDDDRBLBBFBRDLLFDUBLFRLDFUUBFRDBFBBBULFRLBUFLBDDDLLDRBFLLBBLFBFFDFBFDDFRRRBDRRBRBDUFDRLRUDLDFDDURFLBUBBUUDLBRRDUDRDBBBLDBRBBBUFLBLRUURBDDLDRLUFFBLFRLDFBRFLDLBULFFBRLDBDDFLLRFLUBFDFBRLRLFDBLBURLBLFRFBLLDULUDURLBUUULLRRLUBDDLURLLRFURFRFRBDDUBLDFBLUDRLRDRRBLFUFRDUFFRULBLRBBRUFDBUBBBBLDBRBLDDRRFDDBFFUUBRBLFUBBRFUURBFDRLURLRBFUUFUBRUDRBDFBBFURFLFFDRDFUFFULFLUBDFUFFDLRRFRUDUDLBBBDLLLDUFUDRFDBLRRFFLRUFDRFURDLRRDRDLFBRLRLULRFBDLFDRLFRDDFLLDBFBUBBRLLDLFURFRFULUBLUBFLFFBFDFBDUUBURUUUBFUBDLLFLUUUFDUDLUUULDLLUDDBUFRDRULRLLULRULFBLUDFURFLFUBDLLFLFUBUUBBUFLUDUBRDBLFFUUUFDRLRULUDDRLRBLRUUFBRRRRULBDLFBFLDLRDFUBLUBRDDFUULFLDLUBFURRURUBDFFFDLRFFLBRFRDRUDUULURULLDFRBUDRDLFUFULDBLUBFRFBURDLLUUFDURLRDBLFFRFDBFURLFUBLUUUFFRULUBURRURFDDBFUFRBURBBDRFUDDFDLRUURFBBDBDRLUBRRBFDFRDFDLRDUFFUBRRBDBBLDLFDUDDRLFRRRBUUUBRFUFBUFFBRRDRDDBBDRUULDRFRFBUFLFFBLRBFLLLRUDFDRUDLDRLFRLUFLUBRDUFDDLLUDDRBUBBBDRDBBFRBDDRRLRRUUBBUDUDBLDBDFLFRFUBFLFDBBLRLULDBRFBRRLUUURDFFFDBLDUDBRFDDFFUBLUUURBBULFUFUDFBRDLLFURBULULBUDLUFFBDRBRRDBUUULFDURRDFDDLUDBDRBFBUFLULURUFDRFRFBBFBBBDRLBLUDLDRDLLDRRLLDLFBRBRLDUFBDDUDBLDFRFBBBDRDRDDLDRULFFLLFLBLDFLURLBUDFBDLRBLFDFLUDDFUBUBLURBBBLFRLFLBDDBURFFBFRRL",
                "RLURLURBDDULFUUURFLRBLURUBFDBULFLUBBFLDUFBDRFRBRUDFULFRUFLUDFRLFDFLLFDBULURRLBFBUURDULFDFBLRRRLFULLFFFDUULRRRUUUUFDBLDDFFLRDLLUURUBBULUFFURBRRLBBUUBBFDRRBRBRLUDLUDRBFBFULLRRBBFBFRDDDLDDDFRFUFLURUFLBDLUBRLDFRRDBDBFLFUDFLDFFURLFULLDDRURRDLRFLDFLULUUDDRFDRBLRBRBFUFDBDUUDBRRBDFBLBLRBBLBFLLDUBFFFFBDDRLBBBRFDFFUBBDURFLUUDDDRDDLDBRLBULLFLFBRBRBLUDDLRDRDUDFLFRUFLDLBLURDDDRUFDLBRDRLFBDBLDRFBFFBURULUDRRBRDFRFFLULLUBRDRRRDUFRBLFULUBBUFFBRBBFRLFDRRDBLDFRDRDDRLRUULBDURDURFDDLFDUUDBFLBDUFBULFRRDUDUBFBUDBBFUDFUUDLUDDRFDDDFRRRBUDRBFBBULLUFBLRLFLLBRRRRUBDRFLFDFDBLRFLURULULFFBUUUUFDBBLDLUBBRUBBBRBFLULLBLUUULLUBFFDULDFFBFFFUFFDUDRFBUFLDDLURFLRFLRFBUUBLRFDDRULUUUFFRDDBLRDULFURUDDBDLBBUUBFURFRFBRLBUULBLDDDBUBRFFULLUDFFDLDFUBLLBLDFFDDLBDUFUFFLBBBUBULDDFBRRFFLDUDDFRBLRRDDUDLBDBLURBUDBRRLUBBDRFBUFRDRDRBBDULBUFFDRBBDFBUULFFRLLDURRRDFFUUFULDULURLDLUUUDLBBUDLDRFBDBBDLUFBRRFDFLLDLFDBRBBRFUDDDBURDRBUBRUBDUBLDLLDLURLDFDBRUBDLDFRRRBRLULFRFLDRLBUBRUBLFBFDFFLFRFDFLBRULLRBLDRBBFURRRDUUULLULLDLBLBBDFBUUUBRRUFFBRUDBFRDFDLFLFFRFFFFRULDFFDFRUBBBRURBUFLBDFBBBBBRRRLFLFBDRRUFLURDDLRRBRLLFURRURBRFLLLFFURBFULFRFFBLDUUUUBDDUFFDRBRLDDFRBULDDDFFRURUFLDRFLDFBLRUFFUBBDFFDBLLDBDUBDLDLUDFBFLRULRRBDBLRBLDLUURRLLRULDBLBLLRRFDDRBBRBUBDDULDRFBFBBFLUFBLUULDDFDBRLLUBUBBDFBBLBBUBLULDRUDBLRULDUDLUFRRDLLUDDBUFLFLBUFUURFDRDLBURLLRRRULRBFFRRBRFBUBRBUUFRLRDRDLBBRFLLLDDBRFUFRBULFLFDRDDRRDBF",
                "",
                "",
                "",
            };
            //c = new Cube(cs[4]);
            //c = new Cube();
            textboxCubeString.Text = "RFFFUDUDURBFULULFDBLRLDUFDBLUBBBDDURLRDRFRUDDBFUFLFURRLDFRRRUBFUUDUFLLBLBBULDDRRUFUUUBUDFFDRFLRBBLRFDLLUUBBRFRFRLLBFRLBRRFRBDLLDDFBLRDLFBBBLBLBDUUFDDD";
            c = new Cube(textboxCubeString.Text.Trim());
            c3d = new Cube3D(c);
            cw.Cube3D = c3d;
            c3d.AnimationCompleted += C3d_AnimationCompleted;

            ShowWholeCube();
            textboxMoves.Text = "Bw Rw U' Dw' Bw' Fw B Dw Uw2 Bw' Uw' L' Uw F' L Uw2 Bw2 Dw Fw2 L' Rw2 Uw' U' F' Fw2 D' Rw2 R' Bw2 L' Lw2 Dw2 B' U Rw2 F' R D F2 D2 Lw F2 D B R F2 D' B' Lw' F2 R' D F' D Fw L2 U B' L' F L' U' Fw' U L' U R' B Lw B2 L U B2 U B U2 B' Lw' F2 B2 D' Lw' D2 Lw B2 Rw' U2 Rw2 U2 Rw' U2 Rw' U R2 L D2 B' U R F' D' B U' L D2 F2 D B2 D B2 R2 F2 R2";
            initMoves();
        }

        private void btnCreateByCubeString_Click(object sender, RoutedEventArgs e)
        {
            c = new Cube(textboxCubeString.Text);
            c3d = new Cube3D(c);
            cw.Cube3D = c3d;
            c3d.AnimationCompleted += C3d_AnimationCompleted;
            ShowWholeCube();
        }

        private void textboxMoves_TextChanged(object sender, TextChangedEventArgs e)
        {
            initMoves();
        }

        private void initMoves()
        {
            if (listboxMoves != null)
            {
                moves = textboxMoves.Text.Trim().Split(" ");
                listboxMoves.Items.Clear();
                listboxMoves.SelectedIndex = -1;
                foreach (var item in moves)
                {
                    listboxMoves.Items.Add(item);
                }
                currentMoveID = 0;
                autoMoving = false;
                textblockCubeString.Text = "";
            }
        }

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (moves.Length != 0)
            {
                Rotation t;
                t = new Rotation(moves[currentMoveID].Trim());
                currentRotation = t;
                listboxMoves.SelectedIndex = currentMoveID;
                listboxMoves.ScrollIntoView(listboxMoves.SelectedItem);
                if (checkboxAnimation.IsChecked ?? false)
                {
                    int speed = Convert.ToInt32(sliderAnimationSpeed.Value);
                    int stop = Convert.ToInt32(sliderAnimationStop.Value);
                    c3d.Seperate(new Seperation(c3d.Dimension, t));
                    ShowSepertatedCube();
                    c3d.RotateWithAnimation(t, speed, stop);
                }
                else
                {
                    c.Move(t);
                    ShowWholeCube();
                    currentMoveID++;
                    if (currentMoveID == moves.Length)
                    {
                        currentMoveID = 0;
                    }
                }
            }
            textblockCubeString.Text = c.CubeString;
        }

        private void btnAuto_Click(object sender, RoutedEventArgs e)
        {
            textblockCubeString.Text = "";
            if (moves.Length != 0)
            {
                if (checkboxAnimation.IsChecked ?? false)
                {
                    autoMoving = true;
                    Rotation t;
                    t = new Rotation(moves[currentMoveID].Trim());
                    currentRotation = t;
                    listboxMoves.SelectedIndex = currentMoveID;
                    int speed = Convert.ToInt32(sliderAnimationSpeed.Value);
                    int stop = Convert.ToInt32(sliderAnimationStop.Value);
                    c3d.Seperate(new Seperation(c3d.Dimension, t));
                    ShowSepertatedCube();
                    c3d.RotateWithAnimation(t, speed, stop);
                }
                else
                {
                    for (int i = 0; i < moves.Length; i++)
                    {
                        Rotation t;
                        t = new Rotation(moves[i].Trim());
                        c.Move(t);
                    }
                    ShowWholeCube();
                }
            }
        }

        private void C3d_AnimationCompleted(Rotation twist)
        {
            c.Move(currentRotation);
            ShowWholeCube();
            if (autoMoving)
            {
                currentMoveID++;
                if (currentMoveID == moves.Length)
                {
                    currentMoveID = 0;
                    //c3d.AnimationCompleted -= C3d_AnimationCompleted;
                    autoMoving = false;
                }
                else
                {
                    Rotation t;
                    t = new Rotation(moves[currentMoveID].Trim());
                    currentRotation = t;
                    listboxMoves.SelectedIndex = currentMoveID;
                    listboxMoves.ScrollIntoView(listboxMoves.SelectedItem);
                    if (checkboxAnimation.IsChecked ?? false)
                    {
                        int speed = Convert.ToInt32(sliderAnimationSpeed.Value);
                        int stop = Convert.ToInt32(sliderAnimationStop.Value);
                        c3d.Seperate(new Seperation(c3d.Dimension, t));
                        ShowSepertatedCube();
                        c3d.RotateWithAnimation(t, speed, stop);
                    }

                }
            }
            else
            {

            }
        }

        private void ShowWholeCube()
        {
            c3d.getWholeCube();
            cw.ClearContent();
            foreach (var item in c3d.WholeCube)
            {
                cw.CubeModel3DGroup.Children.Add(item);
            }
        }

        private void ShowSepertatedCube()
        {
            cw.ClearContent();
            foreach (var item in c3d.RotatablePart)
            {
                cw.CubeModel3DGroup.Children.Add(item);
            }
            foreach (var item in c3d.FixedPart)
            {
                cw.CubeModel3DGroup.Children.Add(item);
            }
        }


    }
}
