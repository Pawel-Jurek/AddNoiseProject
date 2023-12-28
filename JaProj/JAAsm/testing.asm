.code
main proc
    movups xmm0, [rcx] ; Wczytaj wektor a
    movups xmm1, [rcx + 16] ; Wczytaj wektor b
    movups xmm5, [rcx + 32] ; Wczytaj wektor b
    movups xmm2, [rdx] ; Wczytaj wektor c
    movups xmm3, [r8] ; Wczytaj wektor zT
    movups xmm4, [r9] ; Wczytaj wektor d

    ; Pomnó¿ wektory a, b, c przez zT
    mulps xmm0, xmm3  ; a = a * zT
    mulps xmm1, xmm3  ; b = b * zT
    mulps xmm2, xmm1  ; c = c * zT
    mulps xmm3, xmm4

    movups [rcx], xmm0
    movups [rcx + 8], xmm0
    movups [rdx], xmm1
    movups [r8], xmm2
    movups [r9], xmm3

    ; Zakoñcz program
    mov eax, 0
    ret
main endp
end
