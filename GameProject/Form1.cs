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
        const int JAMSUHAM_SPEED = 8; // 플레이어 캐릭터(잠수함)의 스피드
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
            Graphics g = Graphics.FromImage(hArea); // 그래픽 객체 얻어오기
            g.DrawImage(hSea, 0, 0); // 바다 이미지 그리기
            g.DrawImage(hJamsuham, jX - jW / 2, jY - jH / 2); // 플레이어(잠수함) 그리지


            // 왼쪽 방향키를 누른 상태에서 플레이어를 왼쪽으로 지정한만큼 움직이기
            // 이동 단위는 음수(왼쪽)
            if(GetKeyState((int)Keys.Left) < 0) // GetKeyState : 뭔가 눌리면 음수 반환
            {
                jX = jX - JAMSUHAM_SPEED;
                jX = Math.Max(jW / 2, jX);  // 가장 왼쪽까지 가면 더이상 움직이면 안됨
            }

            // 오른쪽 방향키를 누른 상태에서 플레이어를 왼쪽으로 지정한만큼 움직이기
            // 이동 단위는 양수(오른쪽)
            if (GetKeyState((int)Keys.Right) < 0)
            {
                jX = jX + JAMSUHAM_SPEED;
                jX = Math.Min(ClientSize.Width - jW / 2, jX);  // 가장 dhfms쪽까지 가면 더이상 움직이면 안됨
            }

            Invalidate(); // 화면 전체를 업데이트 하는 기능
        }
    }
}
