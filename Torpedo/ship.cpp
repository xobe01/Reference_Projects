#include "ship.hpp"
#include "graphics.hpp"
#include "widget.hpp"

using namespace genv;
using namespace std;

ship::ship(int x_, int y_, int side_, int frame_,int length_,int direction_):widget(x_,y_,side_, frame_),direction(direction_),length(length_)
{
    selected=false;
}
void ship::draw()
{
    int R,G,B;
    if(selected)
    {
        R=0;
        G=0;
        B=255;
    }
    else
    {
        R=100;
        G=100;
        B=100;
    }
    if(direction==0)
    {
        gout<<move_to(x+1,y+1)<<color(R,G,B)<<box(side*length-2,side-2)<<move_to(x+frame,y+frame)<<color(200,200,200)<<box(side*length-2*frame,side-2*frame);
    }
    else
    {
        gout<<move_to(x+1,y+1)<<color(R,G,B)<<box(side-2,side*length-2)<<move_to(x+frame,y+frame)<<color(200,200,200)<<box(side-2*frame,side*length-2*frame);
    }
}
int ship::getLength()
{
    return length;
}
bool ship::isSelected(int xMouse, int yMouse)
{
    if(direction==0)
    {
        return xMouse>x && xMouse<x+side*length && yMouse>y && yMouse<y+side;
    }
    else
    {
        return xMouse>x && xMouse<x+side && yMouse>y && yMouse<y+side*length;
    }
}
void ship::beSelected()
{
    selected=true;
}
void ship::dontBeSelected()
{
    selected=false;
}
int ship::getX(){}
int ship::getY(){}
void ship::drawAction(int enemyValue){}
