//*********************************************************
//LEDpong.c
//*********************************************************
//Author: 	Robert Yeager
//Start 	Date: 11/24/10
//Company: 	Hill-Rom
//*********************************************************

#define nDebug
#include <pic.h>
#include <stdlib.h>
#include <stdio.h>
#include "LEDpong.h"


__CONFIG(HS & WDTDIS & PWRTDIS & LVPDIS);	//configuration bits

//Chip type   		:PIC16F877A
//Clock frequency	:20 MHz


const char Seg[19]={0x3f,0x06,0x5b,0x4f,0x66,0x6d,0x7d,0x07,0x7f,0x6f,0,0x50,0x01,0x02,0x04,0x08,0x10,0x20,0x30};
				//	0,	 1,	  2,   3,   4,	 5,	  6,   7,	8,   9,	 null, r,  a,  b,   c,   d,   e,   f,   g
const char LED[7]={0x00,0x10,0x20,0x08,0x04,0x02,0x01}; //values 0-6 for LEDS
unsigned long time = 0;						//time in tens of milliseconds
char ledState = 0;							//LED segment state
char sevsegState = 5;						//seven segment display start value
volatile char debounce_counter = 0;			//interrupt time counter flag
char pushed = 0;							//button pushed flag
volatile char btnflag = 0;					//button value flag
volatile char pushedflag = 0;				//button value flag
int timeflag = 0;							//indicates which 7-seg is lit
char blinkflag = 0;						
char stage = 0;								//indicates the current stage of the game 
volatile signed char score[2] = {0,0};				//the in game score
volatile signed char setwin[2] = {BLANK,0};			//sets the winning score
volatile signed char win[2] = {0,0};					//win is used to compare with score
volatile signed char roundnum[2] = {LET_R, 1};		//indicates the current game
volatile signed char *showing = &roundnum;//&setwin;				//holds the address of what is to be shown on the 7-seg
volatile signed char testarray[2] = {8,8};			//used to light up all segments	
volatile signed char soloseg[2] = {12,12};
char blinky = 1;
char segon = 1;
volatile signed char blankstuff[2] = {BLANK,BLANK};
volatile signed char *shadow = &roundnum;
char firsttime = 1;
signed char gameLED = 0;
signed char direction = 1;
char gamestart = 0;
long moretime = FOREVER;
char winLED = 0;
char minspeed = 25;
char up = 1;
unsigned int leds = 0;
char latch = 1;
char speed = 1;

char chgtime = 0;	
char deltime = 0;	
char blinktime = 0;	
char gametime = 0;
char demo = 0;
char demogo = 1;
char GAMEtime = 200;
char BLINKtime = 200;
char DEMOtime = 200;
char rounddelay = 0;
char delaytrig = 0;
char ledtime = 0;

void main(void)
{
	initialize();
	//GREEN = 1;
	leds = 0x8000;
	while(1)
	{	
//*******************************************************************************	
//								SCHEDULED TASKS
//*******************************************************************************	
		if(ledtime)							//while ledtime is true, update LEDs
		{
			ledtime = 0;					//clear ledtime
			LEDOUT =(char) (leds >> 8);		//take first byte of data
			RB0 = 0;						//latch  0;
		}
		if(chgtime)							//while chgtime is true, update 7-seg display
		{
			changeseg(showing);				//update 7-seg display with showing
			chgtime = 0;					//clear chgtime flag
		}
/*		if(rounddelay)
		{
			rounddelay = 0;
			if(pause)
			{
				pause = 0;
				//moretime = FOREVER;
			}
		}
*/		if(blinktime)						//while blinktime is true
		{
			if(blinky)						//while blinky flag is set
			{
				
				if(blinkflag)				//if blinkflag is 1
				{
					showing = &blankstuff;	//set showing to blank, blank
					blinkflag = 0;			//clear blinkflag
					//GREEN = 0;
				}
				else						//if blinkflag is 0
				{
					showing = shadow;		//reset showing with value stored in shadow
					blinkflag = 1;			//set blinkflag
					//GREEN = 1;
				}
			}	
					
			
			if(segon)
			{	
				soloseg[0]++;
				if(soloseg[0] >=18) soloseg[0] = 12;
				soloseg[1]++;
				if(soloseg[1] >=18) soloseg[1] = 12;
			}
			blinktime = 0;					//clear blinktime
		}
		
				
			
		
		if(gametime)
		{
			//blinky = 0;
			gametime = 0;					//clear gametime flag
			//firsttime = 1;
			if(demogo)
			{
				if(direction > 0)			//if the LEDs are moving to the right
				{
					leds = leds >> 1;		//bit shift to the right by 1
					if(leds == 0) 
					{
						if(((GAMEtime >= LEDTIME) || (GAMEtime <= 200)) && (speed == 1))						//can't make LEDs change faster than refresh rate
						{
							direction = -1;
							GAMEtime-=GAMEtime/8;
							leds = 0x0001;
							//if(GAMEtime < 8) GREEN = 1;
							if(GAMEtime <= 8)
							{
								//GAMEtime = 200;
								speed = 0;
							}
						}
/*						else
						{
							GAMEtime = 200;
						}
*/						else if(((GAMEtime <= 8)|| (GAMEtime <= 200)) && (speed == 0))
						{
							//GREEN = 1;
							//while(1);
							direction = -1;
							GAMEtime+=GAMEtime/8;
							leds = 0x0001;
							if(GAMEtime >= 200) 
							{
								//GAMEtime = 200;
								speed = 1;
							}
						}
					}
				}
				if(direction < 0)
				{
					leds = leds << 1;
					if(leds == 0) 
					{
						if(((GAMEtime >= LEDTIME) || (GAMEtime <= 200)) && (speed == 1)) //can't make LEDs change faster than refresh rate
						{
							direction = 1;
							GAMEtime-=GAMEtime/8;
							leds = 0x8000;
							//if(GAMEtime < 8) GREEN = 1;
							if(GAMEtime <= 8) 
							{
								//GAMEtime = 200;
								speed = 0;
							}
						}
/*						else
						{
							GAMEtime = 200;
						}
*/						else if(((GAMEtime <= 8) || (GAMEtime <= 200))&& (speed == 0))
						{
							//GREEN = 1;
							//while(1);
							direction = 1;
							GAMEtime+=GAMEtime/8;
							leds = 0x8000;
							if(GAMEtime >= 200)
							{
								//GAMEtime = 200;
								speed = 1;
							}					
						}
					}
				}
			}
			if(gamestart == 1)				//if gamestart flag is set
			{
				if(direction > 0)			//if the LEDs are moving to the right
				{
					leds = leds >> 1;		//bit shift to the right by 1
					if(leds == 0) 
					{
						if(GAMEtime >= LEDTIME)						//can't make LEDs change faster than refresh rate
						{
							GAMEtime-=GAMEtime/8;
							leds = 0x0001;
							//direction = -1;
							score[0]++;
							firsttime = 1;
							stage = 3;
							gamestart = 0;
						}
						else
						{
							GAMEtime = LEDTIME;
						}						
					}
				}
				if(direction < 0)
				{
					leds = leds << 1;
					if(leds == 0) 
					{
						if(GAMEtime >= LEDTIME) //can't make LEDs change faster than refresh rate
						{
							GAMEtime-=GAMEtime/8;
							leds = 0x8000;
							//direction = 1;
							score[1]++;
							firsttime = 1;
							stage = 3;
							gamestart = 0;
						}
						else
						{
							GAMEtime = LEDTIME;
						}
					}
					
				}
			}
		}
//***********************************************************************************
//									STAGES 		
//***********************************************************************************		
		if(stage == 0)
		{
			if(firsttime)
			{
				BLINKtime = 50;
				blinky = 1;
				segon = 1;
				demogo = 1;
				showing = &soloseg;
				shadow = &soloseg;
				if(ISSET(btnflag,0))					//if btnflag is set by button 1
				{			
					CLRBIT(btnflag,0);
					GREEN = 1;
				}
				if(ISSET(btnflag,1))					//if btnflag is set by button 1
				{			
					CLRBIT(btnflag,1);
					GREEN = 0;
					firsttime = 1;						//set firsttime flag
					stage = 1;
				}
			}
		}
		if(stage == 1) 						//if stage is set to 1
		{
			if(firsttime)					//if firsttime is true (entering this if statement for the first time)
			{
				clear();
				BLINKtime = 200;
				blinky = 0;					//blinky flag not set (function is not set to blink 7-seg)
				segon = 0;
				demogo = 0;
				showing = &setwin;			//showing points to address of setwin
				shadow = &setwin;			//shadow points to address of setwin
				firsttime = 0;					//clear firsttime flag
				winLED = 1;
			}
			pickwin();						//run pickwin function
		}
	
		if(stage == 2) 						//if stage is set to 3
		{
			if(firsttime)					//if firsttime is true (entering this if statement for the first time)
			{
				
				blinky = 0;					//blinky is set (function is set to blink 7-seg)
				segon = 0;
				demogo = 0;
				showing = &score;		//showing points to address of roundnum
				shadow = &score;			//shadow points to address of roundnum
				firsttime = 0;				//clear firsttime flag
				gamestart = 1;
			}
			game();
		}
		if(stage == 3) 
		{
			if(firsttime)					//if firsttime is true (entering this if statement for the first time)
			{
				//moretime = time+ROUNDDELAY;
				blinky = 1;					//blinky is set (function is set to blink 7-seg)
				segon = 0;
				demogo = 0;
				gamestart = 0;
				showing = &score;			//showing points to address of score
				shadow = &score;			//shadow points to address of score
				firsttime = 0;				//clear firsttime flag
				moretime = time + WAIT*2;
			}
			showscore();	
		}
/*		if(stage == 4)						//if stage is set to 5
		{
			if(firsttime)					//if firsttime is true (entering this if statement for the first time)
			{
				blinky = 0;					//blinky is set (function is set to blink 7-seg)
				showing = &testarray;		//showing points to address of testarray
				shadow = &testarray;		//shadow points to address of testarray
				firsttime = 0;				//clear firsttime flag
			}
			test();							//run test function
			
		}
*/	}//end while
}//end main											
//***********************************************************************************
//									GAME FUNCTIONS		
//***********************************************************************************
void pickwin()								//Function used to set the desired winning score
{	

	if(ISSET(btnflag,1))					//if btnflag is set by button 1
	{	
		CLRBIT(btnflag,1);					//clear btnflag for button 2
		firsttime = 1;						//set firsttime flag
		stage = 2;							//set stage to 2
	}
	if(ISSET(btnflag,2))					//if btnflag is set by button 2
	{
		CLRBIT(btnflag,2);					//clear btnflag for button 2
		SEG2 = 1;							//turn on right 7-seg display
		setwin[1]--;						//decrement setwin value [1]
		if(setwin[1]<1) setwin[1]=9;		//if setwin value [1] is decreased below 1, set value [1] to 6
		PORTD = Seg[setwin[1]];				//set PORTD with setwin value [1] corresponding value of Seg array 
	}
	if(ISSET(btnflag,3))					//if btnflag is set by button 3
	{			
		CLRBIT(btnflag,3);					//clear btnflag for button 3
		SEG2 = 1;							//turn on left 7-seg display
		setwin[1]++;						//increment setwin value [1]
		if(setwin[1]>9) setwin[1]=1;		//if setwin value [1] is increased above 6, set value [1] to 1
		PORTD = Seg[setwin[1]];				//set PORTD with setwin value [1] corresponding value of Seg array 
	}
	return;									//exit pickwin function
}

void showscore()								//Function used to compare player scores to setwin, and blink the winner value 
{
	if(score[0] == setwin[1])				//if score value [0] is equal to setwin value [1]
	{
		gamestart = 0;
		direction = -1;
		score[1] = BLANK;					//set score value [1] to blank
		if(ISSET(btnflag,1))					//if btnflag is set by button 1
		{	
			CLRBIT(btnflag,1);					//clear btnflag for button 1
			firsttime = 1;						//set firsttime flag
			stage = 1;							//set stage to 4
			clear();
		}
	}
	else if(score[1] == setwin[1])			//if score value [1] is equal to setwin value [1]
	{
		gamestart = 0;
		direction = 1;
		score[0] = BLANK;	
		if(ISSET(btnflag,1))					//if btnflag is set by button 1
		{	
			CLRBIT(btnflag,1);					//clear btnflag for button 1
			firsttime = 1;						//set firsttime flag
			stage = 1;							//set stage to 4
			clear();
		}
	}
	else
	{	
		if((leds == 0x8000) && (direction < 0))
		{
			if(time >= moretime)
			{
				blinky = 0;
				showing = &score;			//showing points to address of score
				shadow = &score;			//shadow points to address of score
				if(ISSET(btnflag,0))					//if btnflag is set by button 0
				{				
					moretime = FOREVER;
					CLRBIT(btnflag,0);				//clear btnflagfor button 0
					direction = 1;
					stage = 2;
					gamestart = 1;
					firsttime = 1;
				}
			}
		}
		if((leds == 0x0001) && (direction > 0))
		{
			if(time >= moretime)
			{
				blinky = 0;
				showing = &score;			//showing points to address of score
				shadow = &score;			//shadow points to address of score
				if(ISSET(btnflag,3))					//if btnflag is set by button 3
				{	
					moretime = FOREVER;
					CLRBIT(btnflag,3);				//clear btnflagfor button 3
					direction = -1;
					stage = 2;
					gamestart = 1;
					firsttime = 1;
				}
			}
		}
	}
}

void game()
{
	char minspeed = 25;

	if(direction > 0)
	{
		if(ISSET(btnflag,0))					//if btnflag is set by button 0
		{			
			CLRBIT(btnflag,0);					//clear btnflagfor button 0
		}
		if(ISSET(btnflag,3))					//if btnflag is set by button 3
		{			
			CLRBIT(btnflag,3);					//clear btnflagfor button 3
			if(leds == 0x0001)
			{
				direction = -1;
				GAMEtime-=GAMEtime/8;
				//GAMEtime=GAMEtime-minspeed;
			}
		}
	}
	else if(direction < 0)
	{
		if(ISSET(btnflag,3))					//if btnflag is set by button 3
		{			
			CLRBIT(btnflag,3);					//clear btnflagfor button 3
		}
		if(ISSET(btnflag,0))					//if btnflag is set by button 0
		{			
			CLRBIT(btnflag,0);					//clear btnflagfor button 0
			if(leds == 0x8000)
			{
				direction = 1;
				GAMEtime-=GAMEtime/8;
				//GAMEtime=GAMEtime-minspeed;
			}
		}
	}
}
//***********************************************************************************
//									Functions/Drivers/Interrupts		
//***********************************************************************************
void clear()
{
	setwin[1] = 0;
	score[0] = 0;
	score[1] = 0;
	GAMEtime = 200;
	leds = 0x8000;
}

void changeseg(signed char *number)				//7-seg driver
{
	SEG2 = 0;									//turn off right display
	SEG1 = 0;									//turn off left display
	PORTD = Seg[number[timeflag]];				//set PORTD to value passed in, corresponding with Seg array value. Timeflag determines which display is turned on
	if(timeflag) SEG2 = 1;						//if timeflag is 1, turn on right display
	else SEG1 = 1;								//if timeflag is 0, turn on left display
	timeflag ^= 1;								//timeflag toggles
	return;										//exit changeseg
}

void delay(int max)								//delay used for startup sequences
{	

	//int max1 = 255;
	int i,j;
	for(i=0;i<max;i++)							//for loop counts to value of max
	{
		for(j=0;j<WAIT;j++);					//nested for loop counts to value of WAIT
	}
	
	return;										//exit delay function
}	

void checkButtons()								//Button driver
{
	for(int i = 0; i < 4; i++)					//for loop counts up to 4
	{
		if(ISCLR(PORTB,i+4) && ISCLR(pushedflag,i))		//if counter flag = 10
		{	
			SETBIT(btnflag,i);							//
			SETBIT(pushedflag,i);
		}
		if(ISSET(PORTB,i+4) && ISSET(pushedflag,i))
		{	
			CLRBIT(pushedflag,i);
		}
	}
	return;
}

void interrupt ISR()
{
	if(SSPIF)
	{
		SSPIF = 0;	
		if(latch)
		{
			latch = 0;									//clear latch flag
			LEDOUT = (char) (leds & 0x00FF);			//masking the first byte,load second byte of data
		}
		else
		{
			RB0 = 1;									//latch output bit
			latch = 1;									//set latch flag 
		}
	}
	if(TMR2IF)											//watching Timer2 interrupt flag
	{ 
		TMR2IF = 0;										//clear Timer2 interrupt flag
		time++;											//total time counter 
		if(MSEC(CHGTIME)==0) chgtime = 1;				//if scheduled window for CHGTIME occurs, set chgtime flag
		if(MSEC(BLINKtime)==0) blinktime = 1;				//if scheduled window for blinktime occurs, set blinktime flag
		//if(MSEC2(DELTIME)==0) deltime = 1;				//if scheduled window for DELTIME occurs, set deltime flag
		if(MSEC(GAMEtime)==0) gametime = 1;				//if scheduled window for GAMETIME occurs, set gametime flag
		//if(MSEC(DEMOtime)==0) demo = 1;
		//if(time == moretime) rounddelay = 1;
		if(MSEC(LEDTIME)==0) ledtime = 1;				
		if(pushed == 1)									//if pushed flag is set
		{
			debounce_counter++;							//increase PORTB time counter
		}
		if(debounce_counter > DEBOUNCE)					//if PORTB time counter reaches 10 cycles
		{	
			pushed = 0;									//clear pushed flag		
			debounce_counter = 0;						//clear time counter flag
			checkButtons();								//run button driver
		}
//		if (ISSET(btnflag,0)) pickwin();
	}
	if(RBIF)							//watching RB Port Change interrupt flag
	{
		RBIF = 0;						//clear RB Interrupt flag
		pushed = 1;						//clear RB Port Change interrupt flag
	}
	return;
}

void initialize()
{
	PORTC = 0xFF;		//clear PORTC
//Interrupt settings
	GIE = 1;			//enable global interrupts1
	PEIE = 1;			//enable peripheral interrupts
	RBPU = 0;			//enable PORTB pullup resistors
	RBIF = 0;			//clear RB Interrupt flag
	RBIE = 1;			//enable RB port change interrupt
	TMR2IE = 1;			//enables timer2 interrupt	
	SSPIE = 1;			//enables SPI interrupts
//Port settings:
	ADCON1 = 0b00000110;//set PORTA as all digital
	PCFG3=1;			//configure RE2 to RE1 to be digital
	PCFG2=1;
	TRISA = 0b00000000;	//set PORTA as all output
	TRISB = 0b11110000;	//set RB7-4 as input, RB3-0 as output
	//TRISC7 = 1;			//RC7 receives data from PC
	//TRISC6 = 0;			//RC6 sends data to PC
	TRISC5 = 0;			//RC5 SPI Data Out
	TRISC4 = 1;			//RC4 SPI Data In
	TRISC3 = 0;			//RC3 SPI Clock Out
//SPI Settings:	
	SSPSTAT = 0b00000000;		//SMP-0,CKE-0,D/A:BF-0 (I2C stuff)
	SSPCON = 0b00110010;		//WCOL-0,SSPOV-0,SSPEN-1,CKP-1,SSPM3-0,SSPM2-0,SSPM1-1,SSPM0-0	
	TRISD = 0x00; 		//PORTD is output
	TRISE = 0x00; 		//PORTE is output
	PORTA = 0x00;		//clear PORTA
	PORTB = 0x00;		//clear PORTB
	PORTD = 0x00;		//clear PORTD
	PORTE = 0x00;		//clear PORTE
//timer settings	
	T2CON = 0b00101101;			//postscaler = 5, prescalar = 4, timer2on = 1 for 20 MHz crystal
//	T2CON = 0b01100101;			//postscaler = 12, prescalar = 4, timer2on = 1 for 7 MHz crystal
	PR2 = 500;					//1.024 (254 counts) // 1.0031ms (248 counts) 
	TMR2 = 0;					//clear timer counter

	return;
}

