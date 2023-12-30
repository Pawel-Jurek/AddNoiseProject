.data
    negative_two dd -2.0

.code 
colorNoiseAsm PROC
    movups  xmm0, [rcx]         ; wczytanie u1
    
    ;mnozenie xmm0 * (-2)
    movups   xmm1, xmm0 
    movss xmm2, negative_two
    shufps xmm2, xmm2, 0  
    mulps xmm1, xmm2
    sqrtps xmm1, xmm1

    movups xmm0, [rcx + 16]     ; wczytanie u2

    mulps xmm1, xmm0            ; u1*u2 = z
    
    cvtsi2ss xmm0, r9           ; noisePower
    mulps xmm1, xmm0            ; z = z * noisePower

    movups xmm2, [r8]           ; wczytanie colors
    addps xmm2, xmm1            ; z + colors

    movups [rdx], xmm2          
     

    ret

colorNoiseAsm ENDP 
end
