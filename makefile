.PHONY: run

bin/EyeType.exe: EyeType.fs bin
	fsharpc $< -o $@

run: bin/EyeType.exe
	mono --verify-all $<

bin:
	mkdir bin
