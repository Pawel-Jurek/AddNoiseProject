.code
randomNoiseAsm proc
    ; Argumenty funkcji   
    mov r10d, [r8]                 ; dataLength
    mov r12d, [r8 + 4]             ; width
    mov r13d, [r8 + 8]             ; height
    mov r11, r9                    ; coordinatesArray
    mov r8, rcx                    ; pixelRGBs
    mov r9, rdx                    ; data
    
    xor rdi, rdi   ; iterator dla data
    xor rcx, rcx   ; iterator dla coordinatesArray
    
petla_data:
    ; Warunek pêtli dataIterator != dataLenght
    cmp rdi, r10
    jge koniec_petli

    ; Obliczanie indeksu za pomoc¹ wzoru (y * width + height) * 3
    mov eax, [r11 + rcx * 4]    ; x
    inc rcx
    mov ebx, [r11 + rcx * 4]    ; y
    inc rcx

    imul ebx, r12d
    add ebx, eax
    imul ebx, 3


    ; Zapisanie wartoœci dla R, G i B
    movzx rdx, byte ptr [r9 + rdi]
    mov [r8 + rbx], dl
    inc rdi

    movzx rdx, byte ptr [r9 + rdi]
    mov [r8 + rbx + 1], dl
    inc rdi

    movzx rdx, byte ptr [r9 + rdi]
    mov [r8 + rbx + 2], dl
    inc rdi

    jmp petla_data

koniec_petli:
ret
randomNoiseAsm endp
end