#ifndef FIELD_HPP_INCLUDED
#define FIELD_HPP_INCLUDED

#include "graphics.hpp"
#include "widget.hpp"

class field : public widget
{
public:
    field(int x_, int y_, int side_,int frame_);
    virtual void draw();
    virtual void drawAction(int enemyValue);
    virtual int getLength();
    virtual bool isSelected(int xMouse,int yMouse);
    virtual void beSelected();
    virtual void dontBeSelected();
    virtual int getX();
    virtual int getY();
};
#endif // FIELD_HPP_INCLUDED
