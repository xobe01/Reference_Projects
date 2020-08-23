#ifndef SHIP_HPP_INCLUDED
#define SHIP_HPP_INCLUDED

#include "widget.hpp"

class ship : public widget
{
protected:
    int direction,length;
    bool selected;
public:
    ship(int x_,int y_, int side_,int frame_,int length_,int direction_);
    virtual void draw();
    virtual int getLength();
    virtual void drawAction(int enemyValue);
    virtual bool isSelected(int xMouse,int yMouse);
    virtual void beSelected();
    virtual void dontBeSelected();
    virtual int getX();
    virtual int getY();
};

#endif // SHIP_HPP_INCLUDED
