using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Runtime.InteropServices;

namespace GameProject
{
    public partial class Form1 : Form
    {
        // 꼭 정해여쟈 하는 것 (총알 개수, 적군 개수, 스피드, 잠수함 스피드 등등)

        // 설정들이 중간에 변경되지 않도록 상수 처리
        // 적군
        const int SHIP_NUM = 30; // 최대 적군의 배의 수
        // 총알 관련
        const int EGUN_NUM = 50;  // 적군의 총알 발사 최대 개수
        const int EGUN_SPEED = 31; // 적군의 총알 스피드

        // 플레이어
        const int JAMSUHAM_SPEED = 12; // 플레이어 캐릭터(잠수함)의 스피드
        // 총알 관련
        const int JGUN_NUM = 10;  // 플레이어 캐릭터 총알 발사 최대 개수
        const int JGUN_SPEED = 7; // 플레이어 총알 스피드
        const int JGUN_GAP = 40;  // 플레이어 총알사이 간격
      

        // 배 -> 기본적인 몇 가지 속성 (플레이어, 플레이어 총알, 적군 총알)

        // 적군 관련 구조체 정의
        // SHIP 구조체 정의
        struct SHIP
        {
            public bool exist;  // 생사 여부
            public int x, y;    // 위치 좌표
            public int speed;   // 속도
            public int direction;   // 배 방향
        }
        SHIP[] ship = new SHIP[SHIP_NUM];   // 적군 배는 여러 대 출현 가능 배열(30개)

        // 적군 총알 구조체
        struct EGUN
        {
            public bool exist;  // 총알의 존재 여부 (존재하는지, 없어졌는지)
            public int x, y;    // 총알의 위치
        }
        EGUN[] egun = new EGUN[EGUN_NUM];

        // 플레이어 구조체
        // 내 총알 (JAMSUHAM)
        struct JGUN 
        {
            public bool exist;  // 총알의 존재 여부 (존재하는지, 없어졌는지)
            public int x, y;    // 총알의 위치
        }
        JGUN[] jgun = new JGUN[JGUN_NUM];   // 총알 개수 배열 (10개)


        // 플레이어, 적군 속성 정의
        const int jW = 60;  // 플레이어 너비
        const int jH = 35;  // 플레이어 높이
        const int sW = 80;  // 적군 배 너비
        const int sH = 65;  // 적군 배 높이
        const int gw = 6;   // 총알 너비
        const int gH = 16;  // 총알 높이

        int jX = 600;       // 플레이어 위치 X좌표
        int jY = 700;       // 플레이어 위치 Y좌표

        int score = 0;      // 현재 점수

        static int record_score = 0;    // 신기록 / 시작 할 때부터 0이어야 하기 때문에 static으로 정의
        SoundPlayer sndBomb;            // 폭발 소리, 플레이어 or 적이 총알과 부딪혔을 때 사운드

        // 임의의 값을 얻기 위한 기능, 적군 배 개수, 속도 등 처리
        Random random = new Random();

        // 게임 전체 영역에 대한 이미지를 위해 Bitmap 객체
        Bitmap hJamsuham, hShip, hEgun, hJgun, hSea;
        Bitmap hArea = new Bitmap(1200, 800);


        /*
         * 출처 : https://ko.wikipedia.org/wiki/%EC%9C%88%EB%8F%84%EC%9A%B0_%EB%9D%BC%EC%9D%B4%EB%B8%8C%EB%9F%AC%EB%A6%AC_%ED%8C%8C%EC%9D%BC
         * 
         * -- USER32.DLL-- 
         * USER32.DLL은 윈도우 USER 구성 요소를 구현한다. 
         * 윈도우 구성 요소는 창이나 메뉴 같은 윈도우 사용자 인터페이스의 표준 요소들을 생성하고 다룬다. 
         * 그러므로 프로그램들에게 그래픽 사용자 인터페이스(GUI)를 구현할 수 있게 해준다. 
         * 프로그램들은 창 생성이나 관리, 그리고 창 메시지 받기 등을 수행하기 위해 
         * 윈도우 USER에서 함수들을 호출한다.
         * GDI에 관한 많은 USER32.DLL 함수들은 GDI32.DLL에 의해 내보내진 것들이다. 
         * 어떤 종류의 프로그램들은 또한 GDI 함수들을 직접적으로 
         * 호출하여 낮은 수준의 드로잉을 수행하기도 한다.
         * 
         */
        //키 이벤트를 처리하기 위해 필요함
        [DllImport("User32.dll")]

        //키보드로부터 입력한 키값을 얻어오는 윈도우 기반 메소드
        private static extern short GetKeyState(int nVirtKey);


        /*
         * 출처 : https://docs.microsoft.com/ko-kr/dotnet/csharp/programming-guide/interop/how-to-use-platform-invoke-to-play-a-wave-file
         * --winmm.dll--
         * 웨이브 파일을 선택하면 winmm.dll 라이브러리의 PlaySound() 메서드를 사용하여 재생
         */

        //사운드를 처리하기 위해 필요
        [DllImport("winmm.dll")]

        //사운드 음원 재생 및 정지와 같은 기능을 수행하기 위한 윈도우 기반 메소드
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

        

        /*
        * 미디어를 컨트롤할 인터페이스           
        * 첫 번째 매개변수 : 작동 명령
        * 두 번째 매개변수 : 결과 정보를 받을 문자열 변수 지정
        * 세 번째 매개변수 : 두 번째 전달인자에서 지정한 변수에 정보가 들어갈 최대 크기 -> 두 번째가 Null이면 세 번째도 0
        * 네 번째 매개변수 : 함수 처리가 완료 된후 해당 처리를 받을 callback 메소드 지정, 없으면 0
        */

        public Form1()
        {
            InitializeComponent();
        }

        

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = new Size(1200, 800);

            // 리소스 등록 (리소스 등록은 처음 한번만 하면 됨)
            hSea = Properties.Resource.sea; // 리소스를 우리가 쓸 비트맵 변수로 등록
            hJamsuham = Properties.Resource.jamsuham; // 리소스 등록
            hShip = Properties.Resource.ship; // 리소스 등록
            hEgun = Properties.Resource.egun; // 리소스 등록
            hJgun = Properties.Resource.jgun; // 리소스 등록
           
            // 폭발 효과음 등록
            sndBomb = new SoundPlayer(Properties.Resource.bomb);

            StartGame();
        }

        private void StartGame()
        {
            // 처음으로 돌아가면 다시 초기화 해주어야 하는 부분들
            // 적군 배와 총알들의 존재를 false로 초기화
            for (int i = 0; i < SHIP_NUM; i++)
            {
                ship[i].exist = false;
            }
            for (int i = 0; i < EGUN_NUM; i++)
            {
                egun[i].exist = false;
            }
            for (int i = 0; i < JGUN_NUM; i++)
            {
                jgun[i].exist = false;
            }

            // 배경음악 재생
            mciSendString("open \"" + "../../../resource/bg.mp3" + "\" type mpegvideo alias MediaFile", null, 0, IntPtr.Zero);
            mciSendString("play MediaFile REPEAT", null, 0, IntPtr.Zero);

        
            // 스코어 초기화
            score = 0;

            jX = 600;
            // 타이머 시작
            timer1.Start();
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Return:   // 엔터 키(Return) 감지
                    StartGame();    // 다시 시작
                    break;
            }
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (hArea != null)
            {
                e.Graphics.DrawImage(hArea, 0, 0);  // DrawImage() 메서드는 이미지를 출력
                // 여기서는 전체적인 너비의 이미지 영역을 그려줌
                // DrawImage(그릴 이미지, 시작좌표 X, 시작죄표 Y)
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // 배경색으로 지우지 않음
            // 가상 마서드로서 오버라이딩한 형태
            // 이미지를 원래 반복적으로 다시 그려주는데 그럴 때 깜빡임 현상이 일어남
            // 원래의 이 메서드는 화면을 지우는 기능을 함
            // 이 메서드를 오버라이딩 하여 아무런 기능도 하지 않도록 깜빡임 현상 제거
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SHIP local_ship;
            EGUN local_egun;
            JGUN local_jgun;

            Graphics g = Graphics.FromImage(hArea); // 그래픽 객체 얻어오기
            g.DrawImage(hSea, 0, 0); // 바다 이미지 그리기
            g.DrawImage(hJamsuham, jX - jW / 2, jY - jH / 2); // 플레이어(잠수함) 그리지


            int maxY = -1;
            int i, j;

            // 왼쪽 방향키를 누른 상태에서 플레이어를 왼쪽으로 지정한만큼 움직이기
            // 이동 단위는 음수(왼쪽)
            if(GetKeyState((int)Keys.Left) < 0) // GetKeyState : 키가 눌렸을 때 음수 반환
            {
                jX = jX - JAMSUHAM_SPEED;
                jX = Math.Max(jW / 2, jX);  // 가장 왼쪽까지 가면 더이상 움직이면 안됨
            }

            // 오른쪽 방향키를 누른 상태에서 플레이어를 왼쪽으로 지정한만큼 움직이기
            // 이동 단위는 양수(오른쪽)
            if (GetKeyState((int)Keys.Right) < 0)
            {
                jX = jX + JAMSUHAM_SPEED;
                jX = Math.Min(ClientSize.Width - jW / 2, jX);  // 가장 왼쪽까지 가면 더이상 움직이면 안됨
            }


            // 잠수함 총알 발사
            if (GetKeyState((int)Keys.Space) < 0)
            {

                for(i = 0; i < JGUN_NUM; i++)
                {
                    if(jgun[i].exist == true)
                    {
                        maxY = Math.Max(jgun[i].y, maxY);
                    }
                }
                // JGUN_NUM 까지 확인해서 이미 사용하고 있으면 x, 사용을 안하고 있으면 o
                for(i = 0; i < JGUN_NUM; i++)
                {
                    if(jgun[i].exist == false)
                    {
                        break;  // i번째 jgun은 안씀, 써야 함
                    }
                }

                // 총알의 위치를 정의해야함(초기값) => 잠수함의 위치를 생학해줘야 함(x, y)
                // x 잠수함의 위치, y 잠수함의 위치 => 잠수함 그림을 고려 해서 판단 필요

                if (i != JGUN_NUM && jY - maxY > JGUN_GAP)
                {
                    jgun[i].exist = true;   // 새로 총알 발사하는데
                    jgun[i].x = jX;         // 각 값 정의
                    jgun[i].y = jY - gH;    // 각 값 정의
                }
                
            }
            // 총알의 제한 : 개수 한정, 갭이 존재 (너무 빨리 연타가 되면 게임성이 떨어짐)


            // 그려줘야함
            for (i = 0; i < JGUN_NUM; i++)
            {
                if (jgun[i].exist == false) // 총알이 없다면 아래 연산 안하고 건너뜀
                {
                    continue;   
                }
                 
                if(jgun[i].y > 0)   // 총알 높이가 0 이상이면
                {
                    jgun[i].y -= JGUN_SPEED;    // 속도를 점차 줄이면서
                    g.DrawImage(hJgun, jgun[i].x - jW / 2, jgun[i].y);  // 이미지 뿌려주기
                }
                else
                {
                    jgun[i].exist = false; // 높이가 0보다 작으면 존재 안함
                    // Winform에서 총알이 잠수함에서 위쪽으로 날아갈 때
                    // Winfrom 높이는 하단이 큰 수 위쪽으로 갈수록 적어지므로
                    // 총알 높이가 가장 상단(0)이 되면 총알은 더이상 존재하지 않음
                }
            }


            // 기존 게임의 적군이 인공지능 같지만 Random 함수를 사용한 경우가 많음
            // 적군 배 생성 (랜덤)
            if(random.Next(10) == 0)
            {
                for(i = 0; i < SHIP_NUM && ship[i].exist == true; i++) {; }
                // 아래 i 값을 설정해주기 위한 과정
                // 최대 적군 배 값보다 적으면서 현재 존재하는 적군 배의 개수를 확인 한 다음
                // 그 다음에 오는 i값은 exist가 false 이므로
                // 이 for문이 정지되고 i값이 설정됨
                
                if(i != SHIP_NUM)
                {
                    if(random.Next(2)== 1)
                    {
                        ship[i].x = sW;          // 출현 위치
                        ship[i].direction = 1;   // 방향
                    }
                    else
                    {
                        ship[i].x = ClientSize.Width - sW / 2;  // 출현 위치
                        ship[i].direction = -1;                 // 방향
                    }
                    ship[i].y = 300;    // 높이는 항상 고정
                    ship[i].speed = random.Next(4) + 3; // 속도는 랜덤
                    ship[i].exist = true;   // 그 배는 존재하는 것으로 설정 변경
                }
            }


            // [i] -> 어떤 값이 쓰이는지 안쓰이는지를 확인해줘야 함, 속도 랜덤


            // 적군 배 이동 (방향만 지정해주면 왼쪽, 오른쪽으로 - 혹은 +)
            // 적군 총알 생성 (적군 배 i번째의 x, y 가져와서 y값 + 해서 총알이 내려가도록, 900을 벗어나면 false

            for (i = 0; i < SHIP_NUM; i++)
            {
                if (ship[i].exist == false) // 적군 배가 존재 안하면 건너뜀
                {
                    continue;
                }

                ship[i].x += ship[i].speed * ship[i].direction; // 해당 방향과 해당 스피드로 계속 전진

                if (ship[i].x < 0 || ship[i].x > ClientSize.Width)  // 적군 배 x위치가 e보다 작거나, 800보다 크면
                {
                    ship[i].exist = false;   // 사라짐
                }
                else // 아니라면
                {
                    g.DrawImage(hShip, ship[i].x - sW / 2, ship[i].y);  // 계속해서 이미지 그려주기
                }

                // 위치값만 확인해서 생성해서 정해주고 EGUN true
                if(random.Next(60) == 0)
                {
                    for(j = 0; j < EGUN_NUM && egun[j].exist == true; j++) {; }

                    if (j != EGUN_NUM)
                    {
                        egun[j].x = ship[i].x + sW / 2; // 총알 x위치 생성(배의 중앙)
                        egun[j].y = ship[i].y;  // 총알 y위치 생성
                        egun[j].exist = true;   // 총알은 존재하도록 변환
                    }
                }
            }

            // 적군 총알 그려주기
            for (i = 0; i < EGUN_NUM; i++)
            {
                if (egun[i].exist == false) // 총알이 없다면 아래 연산 안하고 건너뜀
                {
                    continue;
                }

                if (egun[i].y < jY)   // 총알이 있는 경우 y위치가 잠수함의 y 위치보다 작은 경우에만
                {
                    egun[i].y += EGUN_SPEED;    // 속도를 계속해서 더해줌
                    g.DrawImage(hEgun, egun[i].x - 3, egun[i].y);  // 이미지 새로 그려주기
                }
                else    // 잠수함의 y 위치보다 크면 사라지게 해야함
                {
                    egun[i].exist = false;  // 사라짐
                }
            }

           
            Rectangle shiprt, jamsuhamrt, egunrt, jgunrt, irt;  // 충돌 계산을 위한 사각형



            // 총알 간 충돌 체크 (내가 맞는 케이스, 적군이 맞는 케이스)

            // 적군과 총알의 충돌 체크
            for(i = 0; i < SHIP_NUM; i++)   // 최대 적군 배 개수 만큼
            {
                if (ship[i].exist == false) // 적군 배가 존재하지 않으면 건너뜀
                {
                    continue;
                }

                local_ship = ship[i];   // ship 하나하나 연산
                int w = sW; // 적군 배 너비 80
                int h = sH; // 적군 배 높이 65

                shiprt = new Rectangle(local_ship.x - w / 2, local_ship.y, w, h);
                // 배의 테두리 정보를 담은 사각형 생성

                for(j = 0; j < JGUN_NUM; j++)   // 내 캐릭터 총알 최대 개수만큼
                {
                    if (jgun[j].exist == false) continue;   // 총알 없으면 건너뜀
                    JGUN b = jgun[j];   // 총알 구조체 하나하나 생성 연산

                    jgunrt = new Rectangle(b.x - gw / 2, b.y, gw, gH);

                    irt = Rectangle.Intersect(shiprt, jgunrt); // 두 사각형의 교차 부분 여부를 체크하는 메서드

                    if(irt.IsEmpty == false)
                    {
                        ship[i].exist = false;  // 배 없앰
                        jgun[j].exist = false;  // 총알 없앰

                        score = score + 10; // 스코어 올리기
                        if(record_score < score)    // 최대 스코어보다 스코어가 크면
                        {
                            record_score = record_score + 10;   // 최대 스코어 갱신
                        }
                        sndBomb.Play();
                    }
                }


                // 점수판 작성 (적군이 맞으면 +, 내가 맞으면 -)
                // 화면에 출력을 하기 위한 부분, 폰트 설정
                Font _font = new System.Drawing.Font(new FontFamily("휴먼둥근헤드라인"), 14, FontStyle.Bold);
                // 문자 그리기(무엇을 쓸지, 폰트 설정, 색상 설정, 위치 설정)
                g.DrawString("Record : " + score.ToString(), _font, Brushes.DarkBlue, new PointF(10, 20));
                g.DrawString("New Record : " + record_score.ToString(), _font, Brushes.DarkBlue, new PointF(500, 20));

                // 적군 총알과 아군의 충돌 판정
                jamsuhamrt = new Rectangle(jX - jW / 2, jY, jW, jH);

                // 내 잠수함 사각형 설정

                for (i = 0; i < EGUN_NUM; i++)   // 적군 총알 최대 개수만큼
                {
                    if (egun[i].exist == false) continue;   // 총알 없으면 건너뜀

                    // 있으면 적군 총알 사각형 찾기
                    egunrt = new Rectangle(egun[i].x - gw / 2, egun[i].y, gw, gH);

                    // 내 배랑 겹치는 부분 찾기
                    irt = Rectangle.Intersect(jamsuhamrt, egunrt); // 두 사각형의 교차 부분 여부를 체크하는 메서드

                    if (irt.IsEmpty == false)   // 겹치는 부분이 비어있지 않다면
                    {
                        mciSendString("stop MediaFile", null, 0, IntPtr.Zero);  // 배경음 중지
                        sndBomb.Play(); // 폭발소리
                        g.Clear(Color.White);
                        GameStop();
                        timer1.Stop();  // 반복 호출되는 타이머를 멈춤
                    }
                }
            }


            // 끝나면 나오는 화면 만들기?
            // 배경화면 다 지우기?
            Invalidate(); // 화면 전체를 업데이트 하는 기능
        }
        private void GameStop()
        {
            Graphics g = Graphics.FromImage(hArea); // 그래픽 객체 얻어오기
            g.DrawImage(hSea, 0, 0); // 바다 이미지 그리기

            Font _font = new System.Drawing.Font(new FontFamily("휴먼둥근헤드라인"),50, FontStyle.Bold);
            // 문자 그리기(무엇을 쓸지, 폰트 설정, 색상 설정, 위치 설정)
            g.DrawString("게임 끝!!", _font, Brushes.DarkBlue, new PointF(450, 400));
        }
    }
}
