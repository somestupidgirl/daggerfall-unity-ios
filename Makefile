# Daggerfall Unity -- iOS touch port -- top-level Makefile (BSD bmake).
#
#	export PATH=/opt/xnuports/bin:$PATH;  bmake
#
# Targets:
#	all		build the signed .ipa (unity -> xcode -> ipa) into Builds/ios/
#	unity		run Unity headless: Addressables + IL2CPP BuildPlayer -> Xcode project
#	xcode		build the .app from the generated Xcode project
#	ipa		package and code-sign the .app into a deployable .ipa
#	check		run the touch-layer self test headlessly (fail-fast gate)
#	unity-install	print what 6000.3.23f1 modules are needed and how to install them
#	clean		remove Builds/ entirely
#
# The iOS pipeline is three stages, each driven from mk/ so the driver Makefile
# here stays a stub -- the same shape as the xnuports trees (see README for the
# layout, mk/*.mk for the build).
#
#	stage 1  mk/unity.mk   -- Unity BuildPlayer emits an Xcode *project*
#	stage 2  mk/xcode.mk   -- xcodebuild turns that project into a .app
#	stage 3  mk/ipa.mk     -- sign + zip the .app into an .ipa
#
# Project pin: 6000.3.23f1 (ProjectSettings/ProjectVersion.txt). Unity 2022.3.62f3
# shipped a Linux-baked iOS IL2CPP backend for macOS-arm64 that hard-fails iOS
# builds, so the port moved to Unity 6 (the iOS pipeline is version-sensitive;
# see mk/dfios.sys.mk for the full rationale).

TOP?=		${.CURDIR}

.include "${TOP}/mk/dfios.sys.mk"

# Stage drivers pull the per-stage Makefile in with the complete configuration
# carried through.  Quote assignments here: both TOP and BUILDDIR may contain
# spaces, and command-line variables are not inherited by recursive bmake runs.
BMAKE?=	bmake
COMMON_MAKE_ARGS= TOP="${TOP}" BUILDDIR="${BUILDDIR}" \
	TESTAPP="${TESTAPP}" DEVBUILD="${DEVBUILD}" \
	SIGN_IDENTITY="${SIGN_IDENTITY}" DEVELOPMENT_TEAM="${DEVELOPMENT_TEAM}" \
	SIGN_CERT="${SIGN_CERT}" XCODE_DEV_DIR="${XCODE_DEV_DIR}" \
	XCODEBUILD_EXTRA_FLAGS="${XCODEBUILD_EXTRA_FLAGS}"
UNITY_MAKE=	${BMAKE} -f "${TOP}/mk/unity.mk" ${COMMON_MAKE_ARGS}
XCODE_MAKE=	${BMAKE} -f "${TOP}/mk/xcode.mk" ${COMMON_MAKE_ARGS}
IPA_MAKE=	${BMAKE} -f "${TOP}/mk/ipa.mk" ${COMMON_MAKE_ARGS}

all: ipa
	@${ECHO} "== dfu-ios build complete =="
	@${ECHO} "   ipa: ${IPA}"

ipa: xcode
	${IPA_MAKE} ipa

xcode: unity
	${XCODE_MAKE} xcode

unity:
	${UNITY_MAKE} unity

check:
	${UNITY_MAKE} check

unity-install:
	${UNITY_MAKE} unity-install

clean:
	rm -rf ${BUILDDIR}

.PHONY: all ipa xcode unity check unity-install clean
