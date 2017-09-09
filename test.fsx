#r "dlls/FsCheck.2.8.0/lib/net452/FsCheck.dll"
#load "ListExtensionsTest.fsx"
#load "LanguagePredictionModelTest.fsx"

module Test =
    open FsCheck
    //Open twice because the fsharp interpreter is retarded (just take my word for it, you'll be happier)
    open ListExtensionsTest
    open ListExtensionsTest

    open LanguagePredictionModelTest
    open LanguagePredictionModelTest

    Check.QuickAll<ListTruncateTo> ()
    Check.QuickAll<PredictionOfFourGram> ()

// vim: set ts=4 sw=4 et:
