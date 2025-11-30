#ifndef RPGGAME_H
#define RPGGAME_H

#include <stdbool.h>
#include <stdint.h>


// --- Forward Declarations ---
typedef struct RpgGame_Damageable RpgGame_Damageable;
typedef struct RpgGame_Named RpgGame_Named;
typedef struct RpgGame_Enemy RpgGame_Enemy;

// --- Struct Definitions ---
struct RpgGame_Damageable
{
    void RpgGame_TakeDamage(T amount)
    { }
    
};

struct RpgGame_Named
{
    char* RpgGame_GetName()
    { }
    
};

struct RpgGame_Enemy
{
    int32_t hp;    
    char* name;    
    void RpgGame_TakeDamage(int32_t amount)
    {
        hp = hp - amount;        
    }
    
    char* RpgGame_GetName()
    {
        return name;        
    }
    
};


// --- Prototypes ---
void RpgGame_Attack(T target, int32_t damage);

#endif // RPGGAME_H
