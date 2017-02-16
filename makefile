.PHONY: run clean

bin/EyeType.exe: listExtensions.fs EyeType.fs bin
	fsharpc listExtensions.fs EyeType.fs -o $@

run: bin/EyeType.exe
	mono --verify-all $<

bin:
	mkdir bin

clean:
	rm -rf bin/*
