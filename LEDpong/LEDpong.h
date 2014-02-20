//******************************************************
//LEDpong.h
//******************************************************
//Author: 	Robert Yeager
//Start 	Date: 11/24/10
//Company: 	Hill-Rom
//******************************************************

//hardware definitions
//--------------------

//#define FREQUENCY  	20		//crystal frequency in MHz
#define BIT0 0x01
#define BIT1 0x02
#define BIT2 0x04
#define BIT3 0x08
#define BIT4 0x10
#define BIT5 0x20
#define BIT6 0x40
#define BIT7 0x80

#define YELLOW1 RE0
#define YELLOW2 BIT5
#define YELLOW3 BIT3
#define RED1 	BIT2
#define RED2	BIT1
#define RED3	BIT0

#define ALL 	YELLOW2|YELLOW3|RED1|RED2|RED3
#define CLEAR	0

#define GREEN	RA0//RC5

#define LEDOUT	SSPBUF		

#define SEG1	RE1
#define SEG2	RE2

#define SEGA	RD0
#define SEGB	RD1
#define SEGC	RD2
#define SEGD	RD3
#define SEGE	RD4
#define SEGF	RD5
#define SEGG	RD6

#define BLANK	10
#define LET_R	11

//#define BLINKTIME 	200//834
#define DELTIME 	16
#define CHGTIME	 	1
#define ROUNDTIME	500
#define WAIT		834//455	
#define BUTTONS		10
#define DEBOUNCE 	5
//#define ROUNDDELAY  1000
#define FOREVER		0xffffffff
#define LEDTIME		5
#define SEGTIME		50
#define LIMIT		8

#define MSEC(x)	time%(x)
#define MSEC2(x) (time & (x-1)) //takes advantage of powers of 2 to do less hardware division

#define CLRBIT(x,y)	x &= ~(1<<y) 

// HOW THIS WORKS:
// variable = 0b01110101;
// CLRBIT(variable,2);
//     variable &= ~(1<<2)
//		 0b01110101 &= ~(0b00000100)
//			0b01110101 &= 0b11111011
//			0b01110001

#define SETBIT(x,y) x |= (1<<y)

// HOW THIS WORKS:
// variable = 0b01110101;
// SETBIT(variable,7);
//     variable |= (1<<7)
//		 0b01110101 |= (0b10000000)
//			0b01110101 1= 0b10000000
//			0b11110101

#define ISSET(x,y)	(x & (1<<y))

// HOW THIS WORKS:
// variable = 0b01110101;
// ISSET(variable,6);
//     variable & (1<<6)
//		 0b01110101 & 0b01000000
//			0b01000000
//			TRUE
// ISSET(variable,3);
//     variable & (1<<3)
//		 0b01110101 & 0b0001000
//			0b00000000
//			FALSE

#define ISCLR(x,y)	!(x & (1<<y))

// HOW THIS WORKS:
// variable = 0b01110101;
// ISCLR(variable,6);
//     !(variable & (1<<6))
//		 !(0b01110101 & 0b01000000)
//			!(0b01000000)
//			FALSE
// ISCLR(variable,3);
//     !(variable & (1<<3))
//		 !(0b01110101 & 0b00000100)
//			!(0b00000000)
//			TRUE

//******************************************************

//Function Prototypes
 
void initialize();		//set up micro registers
void startup();
//void startup1();		//start up sequence #1
//void startup2();		//start up sequence #2
void pickwin();			//set the winning score
//void round();			//signal the round
void game();			//Pong game
void showscore();		//show increase in player score
//void finished();		//once game is over, show winner
void clear();
void changeseg(signed char*);	//driver for 7-seg
//void changeseg2(signed char);	//not used
void changeLED(char);			//driver for LEDs
//void test();					//test function: everything on
void checkButtons();			//button driver
void interrupt ISR();			//includes timer and change of state interrupts
void delay(int);				//delay time - used in start up sequences
//void display(signed char*);
//void display(char,char);