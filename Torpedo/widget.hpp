#ifndef WIDGET_HPP_INCLUDED
#define WIDGET_HPP_INCLUDED
#include "graphics.hpp"

class widget {
protected:
    int x,y,side,frame;

public:
    widget(int x_, int y_, int side_,int frame_);
    virtual bool isSelected(int xMouse, int yMouse)=0;
    virtual void draw()=0;
    virtual void drawAction(int enemyValue)=0;
    virtual int getLength()=0;
    virtual void beSelected()=0;
    virtual void dontBeSelected()=0;
    virtual int getX()=0;
    virtual int getY()=0;
};
#endif // WIDGET_HPP_INCLUDED
