MKDIR:=mkdir.exe -p
CP:=cp.exe -v
AT:=@

##########################################################################

# Remove backslashes. Visual Studio tends to create paths with
# trailing backslashes, which confuses the child make when it tries to
# interpret its command line (e.g., "DEST=x\y\z\ VER=2008" comes out
# as one argument because the space before VER is escaped.)

DEST:=$(subst \,/,$(INSTALL_FOLDER))
SRC:=$(subst \,/,$(OUTPUT_FOLDER))

##########################################################################

all:
	$(error must specify target)

##########################################################################

.PHONY:make_installer
make_installer:
	$(MAKE) make_installer2 INSTALL_FOLDER=$(DEST) OUTPUT_FOLDER=$(SRC) VER=2008
	$(MAKE) make_installer2 INSTALL_FOLDER=$(DEST) OUTPUT_FOLDER=$(SRC) VER=2010
	$(MAKE) make_installer2 INSTALL_FOLDER=$(DEST) OUTPUT_FOLDER=$(SRC) VER=2012
	$(MAKE) make_installer2 INSTALL_FOLDER=$(DEST) OUTPUT_FOLDER=$(SRC) VER=2013

##########################################################################

.PHONY:make_installer2
make_installer2:
	$(MKDIR) "$(DEST)/$(VER)/"
	$(MKDIR) "$(DEST)/$(VER)/minitags/"
	$(CP) -u -t "$(DEST)/$(VER)/minitags/" "$(SRC)/minitags.*" "$(SRC)/quickfind.*" "$(SRC)/vsaddinlib.*"
	$(CP) -u minitags.$(VER).addin "$(DEST)/$(VER)/minitags.addin"

##########################################################################
