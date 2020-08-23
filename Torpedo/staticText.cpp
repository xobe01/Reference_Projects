#include "staticText.hpp"
#include "widget.hpp"
#include <string>

using namespace genv;
using namespace std;

staticText::staticText(int x_, int y_, int side_,int frame_, string wText_) : widget(x_,y_,side_,frame_), wText(wText_){}
void staticText::draw()
{
    gout<<move_to(x,y)<<color(255,255,255)<<text(wText);
}
bool staticText::isSelected(int xMouse, int yMouse){}
void staticText::drawAction(int enemyValue){}
int staticText::getLength(){}
void staticText::beSelected(){}
void staticText::dontBeSelected(){}
int staticText::getX(){}
int staticText::getY(){}
