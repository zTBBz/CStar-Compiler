#ifndef COLLECTIONS_H
#define COLLECTIONS_H

#include <stdbool.h>
#include <stdint.h>


// --- Forward Declarations ---
typedef struct Collections_Vector Collections_Vector;

// --- Struct Definitions ---
struct Collections_Vector
{
    T x;    
    T y;    
    T z;    
    Vector Collections_Create(T x, T y, T z)
    {
        /* var inferred */ int v = /* ? */;        
        /* ? */.x = /* ? */;        
        /* ? */.y = /* ? */;        
        /* ? */.z = /* ? */;        
        return /* ? */;        
    }
    
};


// --- Prototypes ---
void Collections_TestVectors();

#endif // COLLECTIONS_H
