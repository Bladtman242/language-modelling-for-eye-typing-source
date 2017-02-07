.PHONY: run

bin/EyeType.exe: EyeType.fs
	fsharpc $< -o $@

run: bin/EyeType.exe
	mono --verify-all $<
