.PHONY: run clean

bin/EyeType.exe: EyeType.fs bin
	fsharpc EyeType.fs -o $@

run: bin/EyeType.exe
	mono --verify-all $<

bin:
	mkdir bin

clean:
	rm -rf bin/*
