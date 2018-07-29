function ProjectRun()
    let output = system('mono ./src/bin/Debug/removecode.exe ./data/ true')
    call ShowInReadonlyBuffer(output)
endfunction
