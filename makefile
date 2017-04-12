.PHONY: run clean test

bin/EyeType.exe: listExtensions.fs EyeType.fs bin
	fsharpc listExtensions.fs EyeType.fs -o $@

run: bin/EyeType.exe
	mono --verify-all $<

test: test.fsx bin/EyeType.exe
	fsharpi $<

bin:
	mkdir bin

clean:
	rm -rf bin/*
