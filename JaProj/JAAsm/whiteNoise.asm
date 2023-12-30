.data
    negative_two dd -2.0

.code 
whiteNoiseAsm PROC
    movups  xmm0, [rcx]         ; wczytanie u1
    
    ;mnozenie xmm0 * (-2)
    movups   xmm1, xmm0 
    movss xmm2, negative_two
    shufps xmm2, xmm2, 0  
    mulps xmm1, xmm2
    sqrtps xmm1, xmm1

    movups xmm0, [rcx + 16]     ; wczytanie u2
    mulps xmm1, xmm0            ; z = u1*u2
    
    cvtsi2ss xmm0, r8           ; noisePower
    mulps xmm1, xmm0            ; z*=noisePower

    movups xmm2, [rdx]          ; wczytanie r
    movups xmm3, [rdx + 16]     ; wczytanie g
    movups xmm4, [rdx + 32]     ; wczytanie b

    addps xmm2, xmm1            ; r = r + z
    addps xmm3, xmm1            ; g = g + z
    addps xmm4, xmm1            ; b = b + z

    movups [rdx], xmm2          
    movups [rdx + 16], xmm3          
    movups [rdx + 32], xmm4         
    ret

whiteNoiseAsm ENDP 
end
