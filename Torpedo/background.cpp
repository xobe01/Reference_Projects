#include "widget.hpp"
#include "background.hpp"

using namespace genv;

background::background(int x_,int y_,int sideX_,int sideY_,int R_,int G_, int B_):widget(x_,y_,sideX_,sideY_),R(R_),G(G_),B(B_){}
void background::draw()
{
    gout<<move_to(x,y)<<color(R,G,B)<<box(side,frame);
}
bool background::isSelected(int xMouse, int yMouse){}
void background::drawAction(int enemyValue){}
int background::getLength(){}
void background::beSelected(){}
void background::dontBeSelected(){}
int background::getX(){}
int background::getY(){}
