#ifndef BACKGROUND_HPP_INCLUDED
#define BACKGROUND_HPP_INCLUDED
#include "widget.hpp"

class background : public widget
{
private:
    int R,G,B;
public:
    background(int x_, int y_, int sideX_,int sideY_,int R_,int G_, int B_);
    virtual void draw();
    virtual bool isSelected(int xMouse, int yMouse);
    virtual void drawAction(int enemyValue);
    virtual int getLength();
    virtual void beSelected();
    virtual void dontBeSelected();
    virtual int getX();
    virtual int getY();
};

#endif // BACKGROUND_HPP_INCLUDED
