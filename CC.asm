org $8081E4
    JML $9FFF57
    NOP

org $80ED45
    JSL Ammo

org $8081BD
    JSL Input
    NOP

org $8081CF
    JSL Input2
    NOP    

org $9FFF57

    LDA $7FFFF0
    CMP #$ABAB
    BNE +

    LDA #$0000
    STA $7FFFF0
    LDA $7FFFF2
    JSL $80CC3B

    +
    LDA $1EB4
    BNE +
    JML $8081E7
    +
    JML $8081EB

Input:
    LDA $4218
    STA $6E

    LDA $7FFFF6
    CMP #$ABAB
    BNE +

	LDA $6E	
	AND #$F0FF
	STA $7FFFE0
	LDA $6E
	AND #$0A00
	LSR
	ORA $7FFFE0
	STA $7FFFE0

	LDA $6E
	AND #$0500
	ASL
	ORA $7FFFE0
    STA $6E
    +
    LDA $6E
    RTL

Input2:
    LDA $421A
    STA $70

    LDA $7FFFF8
    CMP #$ABAB
    BNE +

	LDA $70
	AND #$F0FF
	STA $7FFFE0
	LDA $70
	AND #$0A00
	LSR
	ORA $7FFFE0
	STA $7FFFE0

	LDA $70
	AND #$0500
	ASL
	ORA $7FFFE0
    STA $70
    +
    LDA $70
    RTL    

Ammo:
    PHA
    LDA $7FFFF4
    CMP #$ABAB
    BEQ +
    PLA
    SEC
    SBC #$0001
    RTL
    +
    PLA
    RTL