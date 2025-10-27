.class final La/a/a/c/KC_c;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements Landroid/os/Parcelable$ClassLoaderCreator;


# annotations
.annotation system Ldalvik/annotation/Signature;
    value = {
        "<T:",
        "Ljava/lang/Object;",
        ">",
        "Ljava/lang/Object;",
        "Landroid/os/Parcelable$ClassLoaderCreator",
        "<TT;>;"
    }
.end annotation


# instance fields
.field private final a:La/a/a/c/KC_b;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "La/a/a/c/KC_b",
            "<TT;>;"
        }
    .end annotation
.end field


# direct methods
.method public constructor <init>(La/a/a/c/KC_b;)V
    .locals 0
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "La/a/a/c/KC_b",
            "<TT;>;)V"
        }
    .end annotation

    .prologue
    .line 31
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 32
    iput-object p1, p0, La/a/a/c/KC_c;->a:La/a/a/c/KC_b;

    .line 33
    return-void
.end method


# virtual methods
.method public final createFromParcel(Landroid/os/Parcel;)Ljava/lang/Object;
    .locals 2
    .param p1, "in"    # Landroid/os/Parcel;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Landroid/os/Parcel;",
            ")TT;"
        }
    .end annotation

    .prologue
    .line 36
    .local p0, "this":La/a/a/c/KC_c;, "La/a/a/c/KC_c<TT;>;"
    iget-object v0, p0, La/a/a/c/KC_c;->a:La/a/a/c/KC_b;

    const/4 v1, 0x0

    invoke-interface {v0, p1, v1}, La/a/a/c/KC_b;->a(Landroid/os/Parcel;Ljava/lang/ClassLoader;)Ljava/lang/Object;

    move-result-object v0

    return-object v0
.end method

.method public final createFromParcel(Landroid/os/Parcel;Ljava/lang/ClassLoader;)Ljava/lang/Object;
    .locals 1
    .param p1, "in"    # Landroid/os/Parcel;
    .param p2, "loader"    # Ljava/lang/ClassLoader;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Landroid/os/Parcel;",
            "Ljava/lang/ClassLoader;",
            ")TT;"
        }
    .end annotation

    .prologue
    .line 40
    .local p0, "this":La/a/a/c/KC_c;, "La/a/a/c/KC_c<TT;>;"
    iget-object v0, p0, La/a/a/c/KC_c;->a:La/a/a/c/KC_b;

    invoke-interface {v0, p1, p2}, La/a/a/c/KC_b;->a(Landroid/os/Parcel;Ljava/lang/ClassLoader;)Ljava/lang/Object;

    move-result-object v0

    return-object v0
.end method

.method public final newArray(I)[Ljava/lang/Object;
    .locals 1
    .param p1, "size"    # I
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(I)[TT;"
        }
    .end annotation

    .prologue
    .line 44
    .local p0, "this":La/a/a/c/KC_c;, "La/a/a/c/KC_c<TT;>;"
    iget-object v0, p0, La/a/a/c/KC_c;->a:La/a/a/c/KC_b;

    invoke-interface {v0, p1}, La/a/a/c/KC_b;->a(I)[Ljava/lang/Object;

    move-result-object v0

    return-object v0
.end method
