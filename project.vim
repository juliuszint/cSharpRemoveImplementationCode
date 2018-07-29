function ProjectRun()
    let output = system('mono ./src/bin/Debug/cSharpRemoveImplementationCode.exe ./data/ true')
    call ShowInReadonlyBuffer(output)
endfunction
