default:
	msbuild /nologo \
			/verbosity:q \
			/property:GenerateFullPaths=true \
			/property:Configuration=Debug \
			/property:AutoGenerateBindingRedirects=true \
			src/cSharpRemoveImplementationCode.sln
