.PHONY: run clean test

bin/EyeType.exe: listExtensions.fs LanguagePredictionModel.fs EyeType.fs bin
	fsharpc listExtensions.fs LanguagePredictionModel.fs EyeType.fs -o $@

run: bin/EyeType.exe
	mono --verify-all $<

test: test.fsx bin/EyeType.exe
	fsharpi $<

content4grams: content4grams.tar.gz
	tar -jxf $<

bin:
	mkdir bin

clean:
	rm -rf bin/*
