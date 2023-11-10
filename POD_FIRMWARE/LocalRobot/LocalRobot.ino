#include <Wire.h>
#include <Arduino.h>
#include <Servo.h>

#include "handlePackets.h"
#include "handleActuators.h"
#include "HandleSensors.h"


Incoming input;

//heartbeat LED
#define LED_TOGGLE_DELAY 500
unsigned long ledToggleTime = 0;
bool ledIsOn = false;

void setup() {
	Serial.begin(9600);
	Serial.println("Starting");	
	delay(100);
	input.setupIncomingPackets();
	setupActuators();
	setupSensors();

	//setup heartbeat
	//pinMode(13, OUTPUT);
	//digitalWrite(13, HIGH);
	//ledToggleTime = millis();

	Serial.println("Initialization Complete\n");

}



void loop() {

	input.handleIncomingPackets();

	handleActuators();

	handleSensors();

	//if (millis() > ledToggleTime + LED_TOGGLE_DELAY) {
	//	ledToggleTime = millis();
	//	if (ledIsOn)
	//		digitalWrite(13, LOW);
	//	else
	//		digitalWrite(13, HIGH);
	//	ledIsOn = !ledIsOn;
	//}
}


/*
* type x5 is analog sensors for servo positioning
* 
* test codes for distance mode setup
* 
1 S0 x2 p0 m0 M10000 e1 t10 T200 
2 S2 x2 p1 m1 M10000 e1 t10 T200
3 A0 x0 p2 t0 c2 T0 e1 
4 A2 x0 p3 t0 c2 T0 e1 

//*****test for pid with distance mode*********
S0 x2 p0 m0 M10000 e1 t10 T200 
S2 x2 p1 m1 M10000 e1 t10 T200 
S1 x3 p0 m1 M10000 e1 t10 T200 
S3 x3 p2 m1 M10000 e1 t10 T200 
A0 x0 p2 t0 c2 T90 e1 
A2 x0 p3 t0 c2 T90 e1 

//****************************This is the sent INIT string as of 3/3/22
A0 x0 p2 t0 c1 T90 e1 
S0 x2 p0 m0 M10000 e1 t10 T200 
S1 x3 p0 m1 M10000 e1 t10 T200 
A1 . 
A2 x0 p3 t0 c1 T90 e1 
A3 . 
S2 x2 p1 m1 M10000 e1 t10 T200 
S3 x3 p2 m1 M10000 e1 t10 T200 
A4 x1 p6 t0 T90 e1 t100 
A5 x1 p7 t0 T90 e1 m90 t100 
S4 x1 p4 m1 e1 t100 T200 
S5 x1 p5 m1 e1 t100 T200 
A6 x1 p9 t0 T160 e1 t3000 
S6 x1 p6 m1 e1 t100 T200 
A7 x1 p10 t0 T135 e1 t3000 
S7 x1 p7 m1 e1 t100 T200 
A8 x1 p8 t0 T60 e1 t3000 
S8 x1 p8 m1 e1 t100 T200 
A9 x1 p11 t0 T90 e1 t3000 
S9 x1 p9 m1 e1 t100 T200 
A10 x1 p12 t 0 T90 e1 t3000 
S10 x1 p10 m1 e1 t100 T200 
A11 x1 p13 t0 T90 e1 t3000 
S11 x1 p11  m1 e1 t100 T200 
S12 x4 p5 t100 T200 e1 m1 
S13 x4 p3 t100 T200 e1 m1 
A0 t1000 T90
 
A2 t1000 T90
 
A4 t1000 T90
 
A5 t1000 T90
 
A6 t1000 T90
 
A7 t1000 T90
 
A8 t1000 T90
 
A9 t1000 T90
 
A10 t1000 T90
 
A11 t1000 T90
//***************************End of Init string
*/

/*
* DC motor with encoder==cheapest I could find
* https://www.aliexpress.com/item/32844684605.html?spm=a2g0o.productlist.0.0.35ee17d6YABPU5&algo_pvid=688c32ae-63d0-4dbb-ba78-d5eaf3854103&algo_exp_id=688c32ae-63d0-4dbb-ba78-d5eaf3854103-6&pdp_ext_f=%7B%22sku_id%22%3A%2265243032782%22%7D&pdp_pi=-1%3B5.99%3B-1%3B-1%40salePrice%3BUSD%3Bsearch-mainSearch
*
* 
* 
*/